using Eshop.Contracts.DTOs;
using Eshop.Core.Models;
using Eshop.Core.Repositories;

namespace Eshop.API.Services
{
    public class WishlistService : IWishlistService
    {
        private readonly IWishlistRepository _wishlistRepository;
        private readonly IProductRepository _productRepository;

        public WishlistService(IWishlistRepository wishlistRepository, IProductRepository productRepository)
        {
            _wishlistRepository = wishlistRepository;
            _productRepository = productRepository;
        }

        public async Task<IEnumerable<WishlistItemDto>> GetByUserIdAsync(string userId)
        {
            var items = await _wishlistRepository.GetByUserIdAsync(userId);
            return items.Select(w => new WishlistItemDto
            {
                Id = w.Id,
                ProductId = w.ProductId,
                ProductName = w.Product?.Name ?? "",
                Price = w.Product?.Price ?? 0,
                StockQuantity = w.Product?.StockQuantity ?? 0,
                AddedAt = w.AddedAt
            });
        }

        public async Task<(bool Success, string Message)> AddAsync(string userId, int productId)
        {
            if (await _wishlistRepository.ExistsAsync(userId, productId))
                return (false, "Product is already in your wishlist.");

            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null)
                return (false, "Product not found.");

            await _wishlistRepository.AddAsync(new WishlistItem
            {
                UserId = userId,
                ProductId = productId,
                AddedAt = DateTime.UtcNow
            });

            return (true, "Added to wishlist.");
        }

        public async Task<bool> RemoveAsync(int wishlistItemId, string userId)
        {
            var item = await _wishlistRepository.GetByIdAsync(wishlistItemId);
            if (item == null || item.UserId != userId)
                return false;

            await _wishlistRepository.DeleteAsync(item);
            return true;
        }

        public async Task<bool> IsInWishlistAsync(string userId, int productId)
        {
            return await _wishlistRepository.ExistsAsync(userId, productId);
        }
    }
}
