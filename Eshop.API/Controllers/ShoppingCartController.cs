using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Eshop.Core.Models;
using Eshop.Web.Data;

namespace Eshop.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ShoppingCartsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ShoppingCartsController(ApplicationDbContext context)
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
    }
}
