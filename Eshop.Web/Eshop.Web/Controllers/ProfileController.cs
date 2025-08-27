using Eshop.Web.Services.Interfaces;
using Eshop.Web.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Eshop.Web.Controllers
{
    public class ProfileController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<IdentityUser> _userManager;

        public ProfileController(IOrderService orderService, IHttpContextAccessor httpContextAccessor, UserManager<IdentityUser> userManager)
        {
            _orderService = orderService;
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
        }

        private string GetUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "guest";
        }

        public async Task<IActionResult> Index(int page = 1)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var pagedOrders = await _orderService.GetOrdersByUserIdAsync(user.Id, page, 10);

            var vm = new ProfileViewModel
            {
                Email = user.Email ?? string.Empty,
                Orders = pagedOrders.Orders,
                TotalOrders = pagedOrders.TotalOrders,
                CurrentPage = page,
                PageSize = 10,
                TotalPages = (int)Math.Ceiling((double)pagedOrders.TotalOrders / 10)
            };

            return View(vm);
        }
    }
}
