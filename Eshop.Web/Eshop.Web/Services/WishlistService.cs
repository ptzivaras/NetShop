using Eshop.Contracts.DTOs;
using Eshop.Web.Services.Interfaces;
using RestSharp;

namespace Eshop.Web.Services
{
    public class WishlistService : IWishlistService
    {
        private readonly RestClient _client;

        public WishlistService(IConfiguration configuration)
        {
            var baseUrl = configuration["ApiSettings:BaseUrl"]
                ?? throw new InvalidOperationException("API BaseUrl not configured.");
            _client = new RestClient(baseUrl);
        }

        public async Task<List<WishlistItemDto>> GetWishlistAsync(string userId)
        {
            var response = await _client.ExecuteAsync<List<WishlistItemDto>>(
                new RestRequest($"wishlist/user/{userId}"));
            return response.Data ?? new List<WishlistItemDto>();
        }

        public async Task<bool> AddAsync(string userId, int productId)
        {
            var response = await _client.ExecuteAsync(
                new RestRequest("wishlist/add", Method.Post)
                    .AddJsonBody(new { UserId = userId, ProductId = productId }));
            return response.IsSuccessful;
        }

        public async Task<bool> RemoveAsync(int wishlistItemId)
        {
            var response = await _client.ExecuteAsync(
                new RestRequest($"wishlist/{wishlistItemId}", Method.Delete));
            return response.IsSuccessful;
        }

        public async Task<bool> IsInWishlistAsync(string userId, int productId)
        {
            var response = await _client.ExecuteAsync<bool>(
                new RestRequest($"wishlist/check/{userId}/{productId}"));
            return response.IsSuccessful && response.Data;
        }
    }
}
