using Eshop.Core.Data;
using Eshop.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Eshop.Core.Repositories
{
    public class WishlistRepository : Repository<WishlistItem>, IWishlistRepository
    {
        public WishlistRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<WishlistItem>> GetByUserIdAsync(string userId)
        {
            return await _dbSet
                .Include(w => w.Product)
                .Where(w => w.UserId == userId)
                .OrderByDescending(w => w.AddedAt)
                .ToListAsync();
        }

        public async Task<WishlistItem?> GetByUserAndProductAsync(string userId, int productId)
        {
            return await _dbSet
                .FirstOrDefaultAsync(w => w.UserId == userId && w.ProductId == productId);
        }

        public async Task<bool> ExistsAsync(string userId, int productId)
        {
            return await _dbSet.AnyAsync(w => w.UserId == userId && w.ProductId == productId);
        }
    }
}
