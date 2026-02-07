using Eshop.Contracts.DTOs;
using Eshop.Core.Models;
using Eshop.Core.Repositories;

namespace Eshop.API.Services
{
    public class ReviewService : IReviewService
    {
        private readonly IReviewRepository _reviewRepository;
        private readonly IProductRepository _productRepository;

        public ReviewService(IReviewRepository reviewRepository, IProductRepository productRepository)
        {
            _reviewRepository = reviewRepository;
            _productRepository = productRepository;
        }

        public async Task<IEnumerable<ReviewDto>> GetReviewsByProductIdAsync(int productId)
        {
            var reviews = await _reviewRepository.GetReviewsByProductIdAsync(productId);
            return reviews.Select(r => new ReviewDto
            {
                Id = r.Id,
                ProductId = r.ProductId,
                ProductName = r.Product?.Name ?? "",
                UserId = r.UserId,
                Rating = r.Rating,
                Comment = r.Comment,
                CreatedAt = r.CreatedAt
            });
        }

        public async Task<IEnumerable<ReviewDto>> GetReviewsByUserIdAsync(string userId)
        {
            var reviews = await _reviewRepository.GetReviewsByUserIdAsync(userId);
            return reviews.Select(r => new ReviewDto
            {
                Id = r.Id,
                ProductId = r.ProductId,
                ProductName = r.Product?.Name ?? "",
                UserId = r.UserId,
                Rating = r.Rating,
                Comment = r.Comment,
                CreatedAt = r.CreatedAt
            });
        }

        public async Task<ReviewDto?> GetReviewByIdAsync(int id)
        {
            var review = await _reviewRepository.GetReviewWithProductAsync(id);
            if (review == null) return null;

            return new ReviewDto
            {
                Id = review.Id,
                ProductId = review.ProductId,
                ProductName = review.Product?.Name ?? "",
                UserId = review.UserId,
                Rating = review.Rating,
                Comment = review.Comment,
                CreatedAt = review.CreatedAt
            };
        }

        public async Task<ReviewDto> CreateReviewAsync(string userId, CreateReviewDto reviewDto)
        {
            var product = await _productRepository.GetByIdAsync(reviewDto.ProductId);
            if (product == null)
                throw new ArgumentException("Product not found");

            var review = new Review
            {
                ProductId = reviewDto.ProductId,
                UserId = userId,
                Rating = reviewDto.Rating,
                Comment = reviewDto.Comment,
                CreatedAt = DateTime.UtcNow
            };

            var createdReview = await _reviewRepository.AddAsync(review);
            
            return new ReviewDto
            {
                Id = createdReview.Id,
                ProductId = createdReview.ProductId,
                ProductName = product.Name,
                UserId = createdReview.UserId,
                Rating = createdReview.Rating,
                Comment = createdReview.Comment,
                CreatedAt = createdReview.CreatedAt
            };
        }

        public async Task<bool> UpdateReviewAsync(int id, string userId, CreateReviewDto reviewDto)
        {
            var review = await _reviewRepository.GetByIdAsync(id);
            if (review == null || review.UserId != userId)
                return false;

            review.Rating = reviewDto.Rating;
            review.Comment = reviewDto.Comment;

            await _reviewRepository.UpdateAsync(review);
            return true;
        }

        public async Task<bool> DeleteReviewAsync(int id, string userId)
        {
            var review = await _reviewRepository.GetByIdAsync(id);
            if (review == null || review.UserId != userId)
                return false;

            await _reviewRepository.DeleteAsync(review);
            return true;
        }
    }
}
