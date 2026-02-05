using Microsoft.AspNetCore.Mvc;
using Eshop.Web.Services;

namespace Eshop.Web.ViewComponents
{
    public class AlertsBadgeViewComponent : ViewComponent
    {
        private readonly IStockAlertService _stockAlertService;

        public AlertsBadgeViewComponent(IStockAlertService stockAlertService)
        {
            _stockAlertService = stockAlertService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            int alertCount = 0;
            
            try
            {
                alertCount = await _stockAlertService.GetUnacknowledgedCountAsync();
            }
            catch (Exception)
            {
                // If API is down, just show 0 alerts
                alertCount = 0;
            }
            
            return View(alertCount);
        }
    }
}
