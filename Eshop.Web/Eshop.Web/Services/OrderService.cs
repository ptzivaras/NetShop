using Eshop.Contracts.DTOs;
using Eshop.Web.Services.Interfaces;
using Eshop.Web.ViewModels;
using RestSharp;

namespace Eshop.Web.Services
{
    public class OrderService : IOrderService
    {
        private readonly RestClient _client;

        public OrderService()
        {
            _client = new RestClient("https://localhost:7068/api");
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
