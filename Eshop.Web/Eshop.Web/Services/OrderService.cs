using Eshop.Contracts.DTOs;
using Eshop.Web.Services.Interfaces;
using Eshop.Web.ViewModels;
using RestSharp;

namespace Eshop.Web.Services
{
    public class OrderService : IOrderService
    {
        private readonly RestClient _client;

        public OrderService(IConfiguration configuration)
        {
            var apiBaseUrl = configuration["ApiSettings:BaseUrl"] 
                ?? throw new InvalidOperationException("API BaseUrl not configured in appsettings.json");
            _client = new RestClient(apiBaseUrl);
        }

        public async Task<OrderDto?> GetOrderByIdAsync(int orderId)
        {
            var request = new RestRequest($"Orders/{orderId}", Method.Get);
            var response = await _client.ExecuteAsync<OrderDto>(request);
            return response.Data;
        }

        public async Task<PagedOrdersViewModel> GetOrdersByUserIdAsync(string userId, int page, int pageSize)
        {
            var request = new RestRequest($"Orders/user/{userId}", Method.Get)
                .AddParameter("page", page)
                .AddParameter("pageSize", pageSize);

            var response = await _client.ExecuteAsync<PagedOrdersViewModel>(request);
            return response.Data ?? new PagedOrdersViewModel();
        }
    }
}
