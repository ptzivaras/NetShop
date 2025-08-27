using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Eshop.Core.Data;
using Eshop.Contracts.DTOs;

namespace Eshop.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public OrdersController(ApplicationDbContext context) => _context = context;

        [HttpPost]
        public async Task<ActionResult<OrderDto>> PlaceOrder([FromBody] OrderDto orderDto)
        {
            var order = new Eshop.Core.Models.Order
            {
                CustomerName = orderDto.CustomerName,
                OrderDate = DateTime.UtcNow,
                OrderItems = orderDto.Items?.Select(i => new Eshop.Core.Models.OrderItem
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice
                }).ToList()
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            orderDto.Id = order.Id;
            return CreatedAtAction(nameof(PlaceOrder), new { id = order.Id }, orderDto);
        }
    }
}
