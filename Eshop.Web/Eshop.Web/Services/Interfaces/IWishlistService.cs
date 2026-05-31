using Eshop.Contracts.DTOs;

namespace Eshop.Web.Services.Interfaces
{
    public interface IWishlistService
    {
        Task<List<WishlistItemDto>> GetWishlistAsync(string userId);
        Task<bool> AddAsync(string userId, int productId);
        Task<bool> RemoveAsync(int wishlistItemId);
        Task<bool> IsInWishlistAsync(string userId, int productId);
    }
}
