using Eshop.Contracts.DTOs;

namespace Eshop.API.Services
{
    public interface IReviewService
    {
        Task<IEnumerable<ReviewDto>> GetReviewsByProductIdAsync(int productId);
        Task<IEnumerable<ReviewDto>> GetReviewsByUserIdAsync(string userId);
        Task<ReviewDto?> GetReviewByIdAsync(int id);
        Task<ReviewDto> CreateReviewAsync(string userId, CreateReviewDto reviewDto);
        Task<bool> UpdateReviewAsync(int id, string userId, CreateReviewDto reviewDto);
        Task<bool> DeleteReviewAsync(int id, string userId);
    }
}
