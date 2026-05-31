using Asp.Versioning;
using Eshop.API.Services;
using Eshop.Contracts.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Eshop.API.Controllers
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [ApiVersion("1.0")]
    [Authorize]
    public class WishlistController : ControllerBase
    {
        private readonly IWishlistService _wishlistService;

        public WishlistController(IWishlistService wishlistService)
        {
            _wishlistService = wishlistService;
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<WishlistItemDto>>> GetByUser(string userId)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (currentUserId != null && currentUserId != userId && !User.IsInRole("Admin"))
                return Forbid();

            var items = await _wishlistService.GetByUserIdAsync(userId);
            return Ok(items);
        }

        [HttpGet("check/{userId}/{productId}")]
        public async Task<ActionResult<bool>> Check(string userId, int productId)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (currentUserId != null && currentUserId != userId && !User.IsInRole("Admin"))
                return Forbid();

            var exists = await _wishlistService.IsInWishlistAsync(userId, productId);
            return Ok(exists);
        }

        [HttpPost("add")]
        public async Task<IActionResult> Add([FromBody] AddToWishlistDto dto)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (currentUserId != null && currentUserId != dto.UserId && !User.IsInRole("Admin"))
                return Forbid();

            var (success, message) = await _wishlistService.AddAsync(dto.UserId, dto.ProductId);
            if (!success)
                return BadRequest(new { message });

            return Ok(new { message });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Remove(int id)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
            var success = await _wishlistService.RemoveAsync(id, currentUserId);
            if (!success)
                return NotFound();

            return NoContent();
        }
    }
}
