using Eshop.Core.Data;
using Eshop.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Eshop.Contracts.DTOs;

namespace Eshop.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ShoppingCartController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ShoppingCartController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ShoppingCart>>> GetCarts()
        {
            return await _context.ShoppingCarts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ShoppingCart>> GetCart(int id)
        {
            var cart = await _context.ShoppingCarts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cart == null)
                return NotFound();

            return cart;
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<ShoppingCartDto>> GetCartByUser(string userId)
        {
            var cart = await _context.ShoppingCarts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
                return NotFound();

            return new ShoppingCartDto
            {
                Id = cart.Id,
                UserId = cart.UserId,
                Items = cart.CartItems.Select(ci => new CartItemDto
                {
                    ProductId = ci.ProductId,
                    ProductName = ci.Product.Name,
                    Price = ci.Product.Price,
                    Quantity = ci.Quantity
                }).ToList()
            };
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddItemToCart([FromBody] CartItemDto request)
        {
            if (request is null || string.IsNullOrWhiteSpace(request.UserId))
                return BadRequest("UserId is required.");
            if (request.ProductId <= 0)
                return BadRequest("Valid ProductId is required.");

            var product = await _context.Products.FindAsync(request.ProductId);
            if (product == null) return NotFound("Product not found.");

            var cart = await _context.ShoppingCarts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == request.UserId);

            if (cart == null)
            {
                cart = new ShoppingCart
                {
                    UserId = request.UserId,
                    CartItems = new List<CartItem>()
                };
                _context.ShoppingCarts.Add(cart);
            }

            var existingItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == request.ProductId);
            if (existingItem != null)
            {
                existingItem.Quantity += 1;
            }
            else
            {
                cart.CartItems.Add(new CartItem
                {
                    ProductId = request.ProductId,
                    Quantity = 1
                });
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                return BadRequest(ex.InnerException?.Message ?? ex.Message);
            }

            var productQty = cart.CartItems.First(ci => ci.ProductId == request.ProductId).Quantity;
            var total = cart.CartItems.Sum(ci => ci.Quantity);
            return Ok(new { productQty, total });
        }

        [HttpPost]
        public async Task<ActionResult<ShoppingCart>> CreateCart(ShoppingCart cart)
        {
            _context.ShoppingCarts.Add(cart);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCart), new { id = cart.Id }, cart);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCart(int id)
        {
            var cart = await _context.ShoppingCarts.FindAsync(id);
            if (cart == null)
                return NotFound();

            _context.ShoppingCarts.Remove(cart);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost("decrease")]
        public async Task<IActionResult> Decrease([FromBody] CartItemDto req)
        {
            var cart = await _context.ShoppingCarts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == req.UserId);

            if (cart == null) return NotFound("Cart not found.");

            var item = cart.CartItems.FirstOrDefault(ci => ci.ProductId == req.ProductId);
            if (item == null) return NotFound("Item not in cart.");

            item.Quantity -= 1;
            if (item.Quantity <= 0)
                _context.CartItems.Remove(item);

            await _context.SaveChangesAsync();

            var productQty = cart.CartItems.FirstOrDefault(ci => ci.ProductId == req.ProductId)?.Quantity ?? 0;
            var total = cart.CartItems.Sum(ci => ci.Quantity);
            return Ok(new { productQty, total });
        }
    }
}
