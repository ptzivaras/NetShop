using Eshop.Core.Models;

namespace Eshop.Core.Repositories
{
    public interface IWishlistRepository : IRepository<WishlistItem>
    {
        Task<IEnumerable<WishlistItem>> GetByUserIdAsync(string userId);
        Task<WishlistItem?> GetByUserAndProductAsync(string userId, int productId);
        Task<bool> ExistsAsync(string userId, int productId);
    }
}
