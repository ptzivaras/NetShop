using Eshop.Contracts.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Eshop.API.Services;
using Eshop.API.Authorization;
using Asp.Versioning;

namespace Eshop.API.Controllers
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [ApiVersion("1.0")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpGet]
        public async Task<ActionResult<PagedResult<OrderDto>>> GetOrders(int page = 1, int pageSize = 20)
        {
            var (orders, total) = await _orderService.GetAllOrdersAsync(page, pageSize);

            return Ok(new PagedResult<OrderDto>
            {
                Items = orders.ToList(),
                TotalCount = total,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)total / pageSize)
            });
        }

        [HttpGet("user/{userId}")]
        [AuthorizeOwnerOrAdmin("userId")]
        public async Task<ActionResult<object>> GetOrdersByUser(string userId, int page = 1, int pageSize = 10)
        {
            var (orders, totalOrders) = await _orderService.GetOrdersByUserIdAsync(userId, page, pageSize);

            return Ok(new
            {
                TotalOrders = totalOrders,
                Orders = orders
            });
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<OrderDto>> GetOrder(int id)
        {
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null)
                return NotFound();

            return Ok(order);
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult> CreateOrder([FromBody] PlaceOrderRequestDto requestDto)
        {
            if (requestDto is null || string.IsNullOrWhiteSpace(requestDto.UserId))
                return BadRequest("Invalid request.");

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");

            // Skip ownership check for anonymous (internal Web layer calls).
            // When JWT auth is added, this will enforce the check properly.
            if (currentUserId != null && currentUserId != requestDto.UserId && !isAdmin)
            {
                return Forbid();
            }

            var (success, message, orderId) = await _orderService.CreateOrderAsync(requestDto.UserId);
            
            if (!success)
                return BadRequest(message);

            return Ok(new { message, orderId });
        }
    }
}
