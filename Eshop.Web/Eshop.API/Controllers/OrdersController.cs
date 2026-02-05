using Eshop.Core.Data;
using Eshop.Core.Models;
using Eshop.Contracts.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Transactions;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Eshop.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public OrdersController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrderDto>>> GetOrders()
        {
            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .ToListAsync();

            var ordersDto = orders.Select(c => new OrderDto
            {
                Id = c.Id,
                UserId = c.UserId,
                OrderDate = c.OrderDate,
                TotalPrice = c.TotalPrice,
                Items = c.OrderItems.Select(ci => new OrderItemDto
                {
                    ProductId = ci.ProductId,
                    ProductName = ci.Product?.Name ?? "",
                    Quantity = ci.Quantity,
                    UnitPrice = ci.UnitPrice
                }).ToList()
            });
            return Ok(ordersDto);
        }

        [HttpGet("user/{userId}")]
        [Authorize]
        public async Task<ActionResult<List<OrderDto>>> GetOrdersByUser(string userId, int page = 1, int pageSize = 10)
        {
            // Security: Verify that the requesting user is either the owner or an Admin
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");
            
            if (currentUserId != userId && !isAdmin)
            {
                return Forbid(); // 403 Forbidden
            }
            
            var query = _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate);

            var totalOrders = await query.CountAsync();

            var orders = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var orderDtos = orders.Select(o => new OrderDto
            {
                Id = o.Id,
                UserId = o.UserId,
                OrderDate = o.OrderDate,
                TotalPrice = o.TotalPrice,
                Items = o.OrderItems.Select(oi => new OrderItemDto
                {
                    ProductId = oi.ProductId,
                    ProductName = oi.Product?.Name ?? "",
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice
                }).ToList()
            }).ToList();

            return Ok(new
            {
                TotalOrders = totalOrders,
                Orders = orderDtos
            });
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<OrderDto>> GetOrder(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
                return NotFound();

            var OrderDto = new OrderDto
            {
                Id = order.Id,
                UserId = order.UserId,
                OrderDate = order.OrderDate,
                TotalPrice = order.TotalPrice,
                OrderId = order.Id,
                Items = order.OrderItems.Select(OrderItem => new OrderItemDto
                {
                    ProductId = OrderItem.ProductId,
                    ProductName = OrderItem.Product.Name,
                    Quantity = OrderItem.Quantity,
                    UnitPrice = OrderItem.UnitPrice
                }).ToList()
            };

            return Ok(OrderDto);
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult> CreateOrder([FromBody] PlaceOrderRequestDto requestDto)
        {
            if (requestDto is null || string.IsNullOrWhiteSpace(requestDto.UserId))
                return BadRequest("Invalid request.");

            var userId = requestDto.UserId;

            var txOptions = new TransactionOptions
            {
                IsolationLevel = IsolationLevel.ReadCommitted,
                Timeout = TransactionManager.DefaultTimeout
            };

            using var scope = new TransactionScope(
                TransactionScopeOption.Required,
                txOptions,
                TransactionScopeAsyncFlowOption.Enabled);

            try
            {
                var cart = await _context.ShoppingCarts
                    .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                    .FirstOrDefaultAsync(c => c.UserId == requestDto.UserId);

                if (cart == null || cart.CartItems == null || !cart.CartItems.Any())
                    return BadRequest("Cart is empty or does not exist.");

                foreach (var item in cart.CartItems)
                {
                    if (item.Product == null)
                        return BadRequest($"Product {item.ProductId} not found.");
                    if (item.Quantity <= 0)
                        return BadRequest($"Invalid quantity for product {item.Product.Name}.");
                    if (item.Quantity > item.Product.StockQuantity)
                        return BadRequest($"Not enough stock for '{item.Product.Name}'.");
                }

                foreach (var item in cart.CartItems)
                {
                    item.Product!.StockQuantity -= item.Quantity;
                }

                var order = new Order
                {
                    UserId = userId,
                    OrderDate = DateTime.UtcNow,
                    OrderItems = cart.CartItems.Select(co => new OrderItem
                    {
                        ProductId = co.ProductId,
                        Quantity = co.Quantity,
                        UnitPrice = co.Product.Price
                    }).ToList(),
                };

                order.TotalPrice = order.OrderItems.Sum(item => item.UnitPrice * item.Quantity);

                _context.Orders.Add(order);
                _context.CartItems.RemoveRange(cart.CartItems);

                await _context.SaveChangesAsync();

                scope.Complete();

                return Ok(new { message = "Order placed successfully", orderId = order.Id });
            }
            catch (DbUpdateException)
            {
                return StatusCode(500, "Database error while placing the order.");
            }
            catch (Exception)
            {
                return StatusCode(500, "Unexpected error while placing the order.");
            }
        }
    }
}
