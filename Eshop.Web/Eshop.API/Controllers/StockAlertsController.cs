using Eshop.Contracts.DTOs;
using Eshop.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Eshop.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class StockAlertsController : ControllerBase
    {
        private readonly IStockAlertService _stockAlertService;

        public StockAlertsController(IStockAlertService stockAlertService)
        {
            _stockAlertService = stockAlertService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<StockAlertDto>>> GetStockAlerts()
        {
            var alerts = await _stockAlertService.GetAllAlertsAsync();
            return Ok(alerts);
        }

        [HttpGet("unacknowledged/count")]
        public async Task<ActionResult<int>> GetUnacknowledgedCount()
        {
            var count = await _stockAlertService.GetUnacknowledgedCountAsync();
            return Ok(count);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<StockAlertDto>> GetStockAlert(int id)
        {
            var alert = await _stockAlertService.GetAlertByIdAsync(id);
            if (alert == null)
                return NotFound();

            return Ok(alert);
        }

        [HttpPut("{id}/acknowledge")]
        public async Task<IActionResult> AcknowledgeAlert(int id)
        {
            var success = await _stockAlertService.AcknowledgeAlertAsync(id);
            if (!success)
                return NotFound();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStockAlert(int id)
        {
            var success = await _stockAlertService.DeleteAlertAsync(id);
            if (!success)
                return NotFound();

            return NoContent();
        }
    }
}
