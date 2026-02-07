using Eshop.Core.Data;
using Eshop.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Eshop.Contracts.DTOs;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Eshop.API.Services;
using Eshop.API.Authorization;

namespace Eshop.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ShoppingCartController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IShoppingCartService _shoppingCartService;

        public ShoppingCartController(ApplicationDbContext context, IShoppingCartService shoppingCartService)
        {
            _context = context;
            _shoppingCartService = shoppingCartService;
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
        [AuthorizeOwnerOrAdmin("userId")]
        public async Task<ActionResult<ShoppingCartDto>> GetCartByUser(string userId)
        {
            var cart = await _shoppingCartService.GetCartByUserIdAsync(userId);
            if (cart == null)
                return NotFound();

            return Ok(cart);
        }

        [HttpPost("add")]
        [Authorize]
        public async Task<IActionResult> AddItemToCart([FromBody] CartItemDto request)
        {
            if (request is null || string.IsNullOrWhiteSpace(request.UserId))
                return BadRequest("UserId is required.");
            if (request.ProductId <= 0)
                return BadRequest("Valid ProductId is required.");

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");
            
            if (currentUserId != request.UserId && !isAdmin)
            {
                return Forbid();
            }

            var (success, message) = await _shoppingCartService.AddItemToCartAsync(request.UserId, request.ProductId, 1);
            
            if (!success)
                return BadRequest(message);

            return Ok(new { message });
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
        [Authorize]
        public async Task<IActionResult> Decrease([FromBody] CartItemDto req)
        {
            if (req is null || string.IsNullOrWhiteSpace(req.UserId))
                return BadRequest("UserId is required.");

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");
            
            if (currentUserId != req.UserId && !isAdmin)
            {
                return Forbid();
            }

            var (success, message) = await _shoppingCartService.DecreaseItemQuantityAsync(req.UserId, req.ProductId, 1);
            
            if (!success)
                return BadRequest(message);

            return Ok(new { message });
        }
    }
}
