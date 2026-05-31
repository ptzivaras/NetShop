using Eshop.API.Services;
using Eshop.Contracts.DTOs;
using Eshop.Core.Models;
using Eshop.Core.Repositories;
using FluentAssertions;
using Moq;

namespace Eshop.UnitTests.Services;

public class ReviewServiceTests
{
    private readonly Mock<IReviewRepository> _mockReviewRepo;
    private readonly Mock<IProductRepository> _mockProductRepo;
    private readonly ReviewService _service;

    public ReviewServiceTests()
    {
        _mockReviewRepo = new Mock<IReviewRepository>();
        _mockProductRepo = new Mock<IProductRepository>();
        _service = new ReviewService(_mockReviewRepo.Object, _mockProductRepo.Object);
    }

    [Fact]
    public async Task GetReviewsByProductIdAsync_ReturnsPagedReviews()
    {
        var reviews = new List<Review>
        {
            new() { Id = 1, ProductId = 5, UserId = "user1", Rating = 5, Comment = "Great!", CreatedAt = DateTime.UtcNow,
                    Product = new Product { Name = "Laptop" } },
            new() { Id = 2, ProductId = 5, UserId = "user2", Rating = 3, Comment = "OK",   CreatedAt = DateTime.UtcNow,
                    Product = new Product { Name = "Laptop" } }
        };
        _mockReviewRepo.Setup(r => r.GetReviewsByProductIdAsync(5, 1, 10)).ReturnsAsync(reviews);
        _mockReviewRepo.Setup(r => r.CountByProductIdAsync(5)).ReturnsAsync(2);

        var (result, total) = await _service.GetReviewsByProductIdAsync(5, 1, 10);

        result.Should().HaveCount(2);
        total.Should().Be(2);
        result.First().Rating.Should().Be(5);
    }

    [Fact]
    public async Task CreateReviewAsync_WhenProductNotFound_ThrowsArgumentException()
    {
        _mockProductRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Product?)null);
        var dto = new CreateReviewDto { ProductId = 99, Rating = 4 };

        var act = async () => await _service.CreateReviewAsync("user1", dto);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*not found*");
    }

    [Fact]
    public async Task CreateReviewAsync_WithValidData_CreatesAndReturnsReview()
    {
        var product = new Product { Id = 5, Name = "Laptop" };
        var dto = new CreateReviewDto { ProductId = 5, Rating = 4, Comment = "Good product" };
        var created = new Review { Id = 1, ProductId = 5, UserId = "user1", Rating = 4, Comment = "Good product", CreatedAt = DateTime.UtcNow };

        _mockProductRepo.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(product);
        _mockReviewRepo.Setup(r => r.AddAsync(It.IsAny<Review>())).ReturnsAsync(created);

        var result = await _service.CreateReviewAsync("user1", dto);

        result.Id.Should().Be(1);
        result.Rating.Should().Be(4);
        result.ProductName.Should().Be("Laptop");
        _mockReviewRepo.Verify(r => r.AddAsync(It.Is<Review>(rv =>
            rv.UserId == "user1" && rv.Rating == 4 && rv.ProductId == 5)), Times.Once);
    }

    [Fact]
    public async Task UpdateReviewAsync_WhenReviewNotFound_ReturnsFalse()
    {
        _mockReviewRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Review?)null);
        var dto = new CreateReviewDto { ProductId = 5, Rating = 3 };

        var result = await _service.UpdateReviewAsync(99, "user1", dto);

        result.Should().BeFalse();
        _mockReviewRepo.Verify(r => r.UpdateAsync(It.IsAny<Review>()), Times.Never);
    }

    [Fact]
    public async Task UpdateReviewAsync_WhenUserDoesNotOwnReview_ReturnsFalse()
    {
        var review = new Review { Id = 1, ProductId = 5, UserId = "other_user", Rating = 5 };
        _mockReviewRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(review);
        var dto = new CreateReviewDto { ProductId = 5, Rating = 3 };

        var result = await _service.UpdateReviewAsync(1, "user1", dto);

        result.Should().BeFalse();
        _mockReviewRepo.Verify(r => r.UpdateAsync(It.IsAny<Review>()), Times.Never);
    }

    [Fact]
    public async Task UpdateReviewAsync_WithValidOwner_UpdatesAndReturnsTrue()
    {
        var review = new Review { Id = 1, ProductId = 5, UserId = "user1", Rating = 5, Comment = "Old" };
        _mockReviewRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(review);
        _mockReviewRepo.Setup(r => r.UpdateAsync(It.IsAny<Review>())).Returns(Task.CompletedTask);
        var dto = new CreateReviewDto { ProductId = 5, Rating = 3, Comment = "Updated" };

        var result = await _service.UpdateReviewAsync(1, "user1", dto);

        result.Should().BeTrue();
        review.Rating.Should().Be(3);
        review.Comment.Should().Be("Updated");
        _mockReviewRepo.Verify(r => r.UpdateAsync(review), Times.Once);
    }

    [Fact]
    public async Task DeleteReviewAsync_WhenUserDoesNotOwnReview_ReturnsFalse()
    {
        var review = new Review { Id = 1, UserId = "other_user" };
        _mockReviewRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(review);

        var result = await _service.DeleteReviewAsync(1, "user1");

        result.Should().BeFalse();
        _mockReviewRepo.Verify(r => r.DeleteAsync(It.IsAny<Review>()), Times.Never);
    }

    [Fact]
    public async Task DeleteReviewAsync_WithValidOwner_DeletesAndReturnsTrue()
    {
        var review = new Review { Id = 1, UserId = "user1" };
        _mockReviewRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(review);
        _mockReviewRepo.Setup(r => r.DeleteAsync(review)).Returns(Task.CompletedTask);

        var result = await _service.DeleteReviewAsync(1, "user1");

        result.Should().BeTrue();
        _mockReviewRepo.Verify(r => r.DeleteAsync(review), Times.Once);
    }
}
