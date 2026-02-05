using Eshop.Contracts.DTOs;
using Eshop.Core.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Eshop.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class StockAlertsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public StockAlertsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/StockAlerts
        [HttpGet]
        public async Task<ActionResult<IEnumerable<StockAlertDto>>> GetStockAlerts()
        {
            var alerts = await _context.StockAlerts
                .OrderByDescending(a => a.TriggeredAt)
                .Select(a => new StockAlertDto
                {
                    Id = a.Id,
                    ProductId = a.ProductId,
                    ProductName = a.ProductName,
                    QuantityAtTrigger = a.QuantityAtTrigger,
                    TriggeredAt = a.TriggeredAt,
                    IsAcknowledged = a.IsAcknowledged
                })
                .ToListAsync();

            return Ok(alerts);
        }

        // GET: api/StockAlerts/unacknowledged/count
        [HttpGet("unacknowledged/count")]
        public async Task<ActionResult<int>> GetUnacknowledgedCount()
        {
            var count = await _context.StockAlerts
                .Where(a => !a.IsAcknowledged)
                .CountAsync();

            return Ok(count);
        }

        // GET: api/StockAlerts/5
        [HttpGet("{id}")]
        public async Task<ActionResult<StockAlertDto>> GetStockAlert(int id)
        {
            var alert = await _context.StockAlerts.FindAsync(id);

            if (alert == null)
            {
                return NotFound();
            }

            var dto = new StockAlertDto
            {
                Id = alert.Id,
                ProductId = alert.ProductId,
                ProductName = alert.ProductName,
                QuantityAtTrigger = alert.QuantityAtTrigger,
                TriggeredAt = alert.TriggeredAt,
                IsAcknowledged = alert.IsAcknowledged
            };

            return Ok(dto);
        }

        // PUT: api/StockAlerts/5/acknowledge
        [HttpPut("{id}/acknowledge")]
        public async Task<IActionResult> AcknowledgeAlert(int id)
        {
            var alert = await _context.StockAlerts.FindAsync(id);
            if (alert == null)
            {
                return NotFound();
            }

            alert.IsAcknowledged = true;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/StockAlerts/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStockAlert(int id)
        {
            var alert = await _context.StockAlerts.FindAsync(id);
            if (alert == null)
            {
                return NotFound();
            }

            _context.StockAlerts.Remove(alert);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
