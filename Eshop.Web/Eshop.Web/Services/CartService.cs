using RestSharp;
using Eshop.Web.ViewModels;
using Eshop.Web.Services.Interfaces;
using Eshop.Contracts.DTOs;
using System.Net;

namespace Eshop.Web.Services
{
    public class CartService : ICartService
    {
        private readonly RestClient _client;

        public CartService(IConfiguration configuration)
        {
            var apiBaseUrl = configuration["ApiSettings:BaseUrl"] 
                ?? throw new InvalidOperationException("API BaseUrl not configured in appsettings.json");
            _client = new RestClient(apiBaseUrl);
        }

        public async Task<ShoppingCartViewModel> GetCartByUserIdAsync(string userId)
        {
            var request = new RestRequest($"shoppingcart/user/{userId}", Method.Get);
            var response = await _client.ExecuteAsync<ShoppingCartViewModel>(request);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return new ShoppingCartViewModel
                {
                    Id = 0,
                    Items = new List<CartItemViewModel>()
                };
            }
            if (!response.IsSuccessful || response.Data == null)
                throw new InvalidOperationException($"API {(int)response.StatusCode}: {response.Content}");

            return response.Data!;
        }

        public async Task AddItemToCartAsync(int productId, string userId)
        {
            var request = new RestRequest("shoppingcart/add", Method.Post);
            request.AddJsonBody(new { UserId = userId, ProductId = productId });

            var response = await _client.ExecuteAsync(request);
            if (!response.IsSuccessful)
                throw new InvalidOperationException($"API {(int)response.StatusCode}: {response.Content}");
        }

        public async Task<int> PlaceOrderAsync(string userId)
        {
            var request = new RestRequest("orders", Method.Post);
            request.AddJsonBody(new { UserId = userId });

            var response = await _client.ExecutePostAsync<OrderDto>(request);
            if (!response.IsSuccessful || response.Data is null)
                throw new InvalidOperationException($"API {(int)response.StatusCode}: {response.Content}");

            return response.Data.OrderId;
        }

        private class AddCartResponse
        {
            public int productQty { get; set; }
            public int total { get; set; }
        }

        public async Task<int> AddItemToCartAndReturnQuantityAsync(int productId, string userId)
        {
            var request = new RestRequest("shoppingcart/add", Method.Post)
                .AddJsonBody(new { UserId = userId, ProductId = productId });

            var response = await _client.ExecuteAsync<AddCartResponse>(request);
            if (!response.IsSuccessful || response.Data == null)
                throw new InvalidOperationException($"API {(int)response.StatusCode}: {response.Content}");

            return response.Data.productQty;
        }

        private class QtyResponse
        {
            public int productQty { get; set; }
            public int total { get; set; }
        }

        public async Task DecreaseItemAsync(int productId, string userId)
        {
            var req = new RestRequest("shoppingcart/decrease", Method.Post)
                .AddJsonBody(new { UserId = userId, ProductId = productId });

            var resp = await _client.ExecuteAsync<QtyResponse>(req);
            if (!resp.IsSuccessful || resp.Data is null)
                throw new InvalidOperationException($"API {(int)resp.StatusCode}: {resp.Content}");
        }
    }
}
