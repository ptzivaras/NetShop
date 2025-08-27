using Eshop.Web.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Eshop.Web.Controllers
{
    public class OrdersController : Controller
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Confirmation(int orderId)
        {
            var order = await _orderService.GetOrderByIdAsync(orderId);
            if (order == null)
            {
                TempData["Error"] = "Order not found.";
                return RedirectToAction("Index", "Cart");
            }
            return View("OrderConfirmation", order);
        }
    }
}
