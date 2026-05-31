using Eshop.Contracts.DTOs;

namespace Eshop.Web.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalProducts { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public int UnacknowledgedAlerts { get; set; }
        public int OrdersThisMonth { get; set; }
        public decimal RevenueThisMonth { get; set; }
        public List<OrderDto> RecentOrders { get; set; } = new();
        public List<MonthlyStatDto> MonthlyStats { get; set; } = new();
    }
}
