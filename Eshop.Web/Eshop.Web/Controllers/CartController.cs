using Microsoft.AspNetCore.Mvc;
using Eshop.Web.Services.Interfaces;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Eshop.Contracts.DTOs;
using Microsoft.AspNetCore.Cors.Infrastructure;

namespace Eshop.Web.Controllers
{
    public class CartController : Controller
    {
        private readonly ICartService _cartService;
        private readonly UserManager<IdentityUser> _userManager;
        private string GetUserIdStrict() => _userManager.GetUserId(User)!;

        public CartController(ICartService cartService, UserManager<IdentityUser> userManager)
        {
            _cartService = cartService;
            _userManager = userManager;
        }

        private string GetUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "guest";
        }

        public async Task<IActionResult> Index()
        {
            if (!User.Identity?.IsAuthenticated ?? true) return Challenge();
            var userId = GetUserIdStrict();
            var cart = await _cartService.GetCartByUserIdAsync(userId);
            return View(cart);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(int productId)
        {
            if (!User.Identity?.IsAuthenticated ?? true)
                return Unauthorized(new { message = "Login required" });

            var userId = GetUserIdStrict();

            try
            {
                var qty = await _cartService.AddItemToCartAndReturnQuantityAsync(productId, userId);
                return Json(new { productId, quantity = qty });
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(502, new { message = "Cart API unreachable", detail = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = "Cart API error", detail = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Unexpected error", detail = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> PlaceOrder()
        {
            var userId = GetUserId();

            try
            {
                var orderId = await _cartService.PlaceOrderAsync(userId);
                TempData["Message"] = "Order placed successfully!";
                return RedirectToAction("Confirmation", "Orders", new { orderId });
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Order failed: {ex.Message}";
                return RedirectToAction("Index", "Cart");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Decrease(int productId)
        {
            if (!User.Identity?.IsAuthenticated ?? true) return Challenge();
            var userId = GetUserIdStrict();

            try
            {
                await _cartService.DecreaseItemAsync(productId, userId);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Decrease failed: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Increase(int productId)
        {
            if (!User.Identity?.IsAuthenticated ?? true) return Challenge();

            var userId = GetUserIdStrict();

            try
            {
                await _cartService.AddItemToCartAsync(productId, userId);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Increase failed: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
