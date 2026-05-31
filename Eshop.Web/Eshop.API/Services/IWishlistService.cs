using Eshop.Contracts.DTOs;

namespace Eshop.API.Services
{
    public interface IWishlistService
    {
        Task<IEnumerable<WishlistItemDto>> GetByUserIdAsync(string userId);
        Task<(bool Success, string Message)> AddAsync(string userId, int productId);
        Task<bool> RemoveAsync(int wishlistItemId, string userId);
        Task<bool> IsInWishlistAsync(string userId, int productId);
    }
}
