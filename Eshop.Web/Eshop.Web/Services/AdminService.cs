using Eshop.Contracts.DTOs;
using Eshop.Web.Services.Interfaces;
using Eshop.Web.ViewModels;
using RestSharp;

namespace Eshop.Web.Services
{
    public class AdminService : IAdminService
    {
        private readonly RestClient _client;

        public AdminService(IConfiguration configuration)
        {
            var baseUrl = configuration["ApiSettings:BaseUrl"]
                ?? throw new InvalidOperationException("API BaseUrl not configured.");
            _client = new RestClient(baseUrl);
        }

        public async Task<AdminDashboardViewModel> GetDashboardAsync()
        {
            var productsTask = _client.ExecuteAsync<PagedResult<ProductDto>>(
                new RestRequest("products").AddQueryParameter("page", 1).AddQueryParameter("pageSize", 1));

            var statsTask = _client.ExecuteAsync<OrderStatsDto>(
                new RestRequest("orders/stats"));

            var recentOrdersTask = _client.ExecuteAsync<PagedResult<OrderDto>>(
                new RestRequest("orders").AddQueryParameter("page", 1).AddQueryParameter("pageSize", 5));

            var alertsTask = _client.ExecuteAsync<int>(
                new RestRequest("stockalerts/unacknowledged/count"));

            await Task.WhenAll(productsTask, statsTask, recentOrdersTask, alertsTask);

            var stats = statsTask.Result.Data;
            var recentOrders = recentOrdersTask.Result.Data;

            return new AdminDashboardViewModel
            {
                TotalProducts = productsTask.Result.Data?.TotalCount ?? 0,
                TotalOrders = stats?.TotalOrders ?? 0,
                TotalRevenue = stats?.TotalRevenue ?? 0,
                OrdersThisMonth = stats?.OrdersThisMonth ?? 0,
                RevenueThisMonth = stats?.RevenueThisMonth ?? 0,
                UnacknowledgedAlerts = alertsTask.Result.IsSuccessful ? alertsTask.Result.Data : 0,
                RecentOrders = recentOrders?.Items?.ToList() ?? new List<OrderDto>(),
                MonthlyStats = stats?.MonthlyStats ?? new List<MonthlyStatDto>()
            };
        }
    }
}
