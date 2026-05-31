namespace Eshop.Contracts.DTOs
{
    public class OrderStatsDto
    {
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public int OrdersThisMonth { get; set; }
        public decimal RevenueThisMonth { get; set; }
        public List<MonthlyStatDto> MonthlyStats { get; set; } = new();
    }

    public class MonthlyStatDto
    {
        public string Label { get; set; } = "";
        public int OrderCount { get; set; }
        public decimal Revenue { get; set; }
    }
}
