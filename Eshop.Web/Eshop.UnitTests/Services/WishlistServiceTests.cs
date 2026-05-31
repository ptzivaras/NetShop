using Eshop.API.Services;
using Eshop.Core.Models;
using Eshop.Core.Repositories;
using FluentAssertions;
using Moq;

namespace Eshop.UnitTests.Services;

public class WishlistServiceTests
{
    private readonly Mock<IWishlistRepository> _mockWishlistRepo;
    private readonly Mock<IProductRepository> _mockProductRepo;
    private readonly WishlistService _service;

    public WishlistServiceTests()
    {
        _mockWishlistRepo = new Mock<IWishlistRepository>();
        _mockProductRepo = new Mock<IProductRepository>();
        _service = new WishlistService(_mockWishlistRepo.Object, _mockProductRepo.Object);
    }

    [Fact]
    public async Task GetByUserIdAsync_ReturnsUserWishlistItems()
    {
        var userId = "user1";
        var items = new List<WishlistItem>
        {
            new() { Id = 1, UserId = userId, ProductId = 10, AddedAt = DateTime.UtcNow,
                    Product = new Product { Id = 10, Name = "Laptop", Price = 1200, StockQuantity = 5 } },
            new() { Id = 2, UserId = userId, ProductId = 20, AddedAt = DateTime.UtcNow,
                    Product = new Product { Id = 20, Name = "Mouse", Price = 25, StockQuantity = 50 } }
        };
        _mockWishlistRepo.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(items);

        var result = await _service.GetByUserIdAsync(userId);

        result.Should().HaveCount(2);
        result.First().ProductName.Should().Be("Laptop");
        result.First().Price.Should().Be(1200);
    }

    [Fact]
    public async Task AddAsync_WhenProductAlreadyInWishlist_ReturnsFailure()
    {
        _mockWishlistRepo.Setup(r => r.ExistsAsync("user1", 10)).ReturnsAsync(true);

        var (success, message) = await _service.AddAsync("user1", 10);

        success.Should().BeFalse();
        message.Should().Contain("already in your wishlist");
        _mockWishlistRepo.Verify(r => r.AddAsync(It.IsAny<WishlistItem>()), Times.Never);
    }

    [Fact]
    public async Task AddAsync_WhenProductNotFound_ReturnsFailure()
    {
        _mockWishlistRepo.Setup(r => r.ExistsAsync("user1", 99)).ReturnsAsync(false);
        _mockProductRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Product?)null);

        var (success, message) = await _service.AddAsync("user1", 99);

        success.Should().BeFalse();
        message.Should().Contain("not found");
        _mockWishlistRepo.Verify(r => r.AddAsync(It.IsAny<WishlistItem>()), Times.Never);
    }

    [Fact]
    public async Task AddAsync_WithValidData_AddsItemAndReturnsSuccess()
    {
        var product = new Product { Id = 10, Name = "Laptop", Price = 1200, StockQuantity = 5 };
        _mockWishlistRepo.Setup(r => r.ExistsAsync("user1", 10)).ReturnsAsync(false);
        _mockProductRepo.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(product);
        _mockWishlistRepo.Setup(r => r.AddAsync(It.IsAny<WishlistItem>()))
            .ReturnsAsync(new WishlistItem { Id = 1 });

        var (success, message) = await _service.AddAsync("user1", 10);

        success.Should().BeTrue();
        message.Should().Contain("Added");
        _mockWishlistRepo.Verify(r => r.AddAsync(It.Is<WishlistItem>(w =>
            w.UserId == "user1" && w.ProductId == 10)), Times.Once);
    }

    [Fact]
    public async Task RemoveAsync_WhenItemNotFound_ReturnsFalse()
    {
        _mockWishlistRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((WishlistItem?)null);

        var result = await _service.RemoveAsync(99, "user1");

        result.Should().BeFalse();
        _mockWishlistRepo.Verify(r => r.DeleteAsync(It.IsAny<WishlistItem>()), Times.Never);
    }

    [Fact]
    public async Task RemoveAsync_WhenItemBelongsToDifferentUser_ReturnsFalse()
    {
        var item = new WishlistItem { Id = 1, UserId = "other_user", ProductId = 10 };
        _mockWishlistRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(item);

        var result = await _service.RemoveAsync(1, "user1");

        result.Should().BeFalse();
        _mockWishlistRepo.Verify(r => r.DeleteAsync(It.IsAny<WishlistItem>()), Times.Never);
    }

    [Fact]
    public async Task RemoveAsync_WhenItemExistsAndBelongsToUser_DeletesAndReturnsTrue()
    {
        var item = new WishlistItem { Id = 1, UserId = "user1", ProductId = 10 };
        _mockWishlistRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(item);
        _mockWishlistRepo.Setup(r => r.DeleteAsync(item)).Returns(Task.CompletedTask);

        var result = await _service.RemoveAsync(1, "user1");

        result.Should().BeTrue();
        _mockWishlistRepo.Verify(r => r.DeleteAsync(item), Times.Once);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task IsInWishlistAsync_ReturnsCorrectResult(bool exists)
    {
        _mockWishlistRepo.Setup(r => r.ExistsAsync("user1", 10)).ReturnsAsync(exists);

        var result = await _service.IsInWishlistAsync("user1", 10);

        result.Should().Be(exists);
    }
}
