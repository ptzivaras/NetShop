using Eshop.Core.Models;

namespace Eshop.Core.Repositories
{
    public interface IReviewRepository : IRepository<Review>
    {
        Task<IEnumerable<Review>> GetReviewsByProductIdAsync(int productId);
        Task<IEnumerable<Review>> GetReviewsByUserIdAsync(string userId);
        Task<Review?> GetReviewWithProductAsync(int id);
    }
}
