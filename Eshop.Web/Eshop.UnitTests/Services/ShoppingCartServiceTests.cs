using Eshop.API.Services;
using Eshop.Core.Models;
using Eshop.Core.Repositories;
using FluentAssertions;
using Moq;

namespace Eshop.UnitTests.Services;

public class ShoppingCartServiceTests
{
    private readonly Mock<IShoppingCartRepository> _mockCartRepository;
    private readonly Mock<IProductRepository> _mockProductRepository;
    private readonly ShoppingCartService _shoppingCartService;

    public ShoppingCartServiceTests()
    {
        _mockCartRepository = new Mock<IShoppingCartRepository>();
        _mockProductRepository = new Mock<IProductRepository>();
        _shoppingCartService = new ShoppingCartService(
            _mockCartRepository.Object,
            _mockProductRepository.Object);
    }

    [Fact]
    public async Task GetCartByUserIdAsync_WithExistingCart_ReturnsCart()
    {
        // Arrange
        var userId = "user123";
        var product = new Product { Id = 1, Name = "Laptop", Price = 1000 };
        var cart = new ShoppingCart
        {
            Id = 1,
            UserId = userId,
            CartItems = new List<CartItem>
            {
                new() { ProductId = 1, Quantity = 2, Product = product }
            }
        };
        _mockCartRepository.Setup(r => r.GetCartWithItemsAsync(userId))
            .ReturnsAsync(cart);

        // Act
        var result = await _shoppingCartService.GetCartByUserIdAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result!.UserId.Should().Be(userId);
        result.Items.Should().HaveCount(1);
        result.Items.First().ProductName.Should().Be("Laptop");
        result.Items.First().Quantity.Should().Be(2);
    }

    [Fact]
    public async Task GetCartByUserIdAsync_WithNonExistingCart_ReturnsNull()
    {
        // Arrange
        _mockCartRepository.Setup(r => r.GetCartWithItemsAsync("nonexistent"))
            .ReturnsAsync((ShoppingCart?)null);

        // Act
        var result = await _shoppingCartService.GetCartByUserIdAsync("nonexistent");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task AddItemToCartAsync_WithInvalidUserId_ReturnsFailure()
    {
        // Act
        var result = await _shoppingCartService.AddItemToCartAsync("", 1, 1);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("User ID is required");
    }

    [Fact]
    public async Task AddItemToCartAsync_WithInvalidProductId_ReturnsFailure()
    {
        // Act
        var result = await _shoppingCartService.AddItemToCartAsync("user123", 0, 1);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Valid Product ID is required");
    }

    [Fact]
    public async Task AddItemToCartAsync_WithNonExistentProduct_ReturnsFailure()
    {
        // Arrange
        _mockProductRepository.Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((Product?)null);

        // Act
        var result = await _shoppingCartService.AddItemToCartAsync("user123", 999, 1);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Product not found");
    }

    [Fact]
    public async Task AddItemToCartAsync_ToNewCart_CreatesCartAndAddsItem()
    {
        // Arrange
        var userId = "user123";
        var product = new Product { Id = 1, Name = "Mouse", Price = 25 };
        var newCart = new ShoppingCart
        {
            Id = 1,
            UserId = userId,
            CartItems = new List<CartItem>()
        };

        _mockProductRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(product);
        
        // First call returns null (no existing cart)
        var getCartCalls = 0;
        _mockCartRepository.Setup(r => r.GetCartWithItemsAsync(userId))
            .ReturnsAsync(() =>
            {
                getCartCalls++;
                return getCartCalls == 1 ? null : newCart; // null first, then newCart after AddAsync
            });
        
        _mockCartRepository.Setup(r => r.AddAsync(It.IsAny<ShoppingCart>()))
            .ReturnsAsync(newCart);
        _mockCartRepository.Setup(r => r.UpdateAsync(It.IsAny<ShoppingCart>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _shoppingCartService.AddItemToCartAsync(userId, 1, 2);

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Contain("added to cart successfully");
        _mockCartRepository.Verify(r => r.AddAsync(It.Is<ShoppingCart>(c => c.UserId == userId)), Times.Once);
        _mockCartRepository.Verify(r => r.UpdateAsync(It.IsAny<ShoppingCart>()), Times.Once);
    }

    [Fact]
    public async Task AddItemToCartAsync_ToExistingCartWithNewProduct_AddsNewItem()
    {
        // Arrange
        var userId = "user123";
        var product = new Product { Id = 2, Name = "Keyboard", Price = 50 };
        var cart = new ShoppingCart
        {
            Id = 1,
            UserId = userId,
            CartItems = new List<CartItem>
            {
                new() { ProductId = 1, Quantity = 1, Product = new Product { Id = 1 } }
            }
        };

        _mockProductRepository.Setup(r => r.GetByIdAsync(2))
            .ReturnsAsync(product);
        _mockCartRepository.Setup(r => r.GetCartWithItemsAsync(userId))
            .ReturnsAsync(cart);
        _mockCartRepository.Setup(r => r.UpdateAsync(It.IsAny<ShoppingCart>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _shoppingCartService.AddItemToCartAsync(userId, 2, 3);

        // Assert
        result.Success.Should().BeTrue();
        cart.CartItems.Should().HaveCount(2);
        cart.CartItems.Last().ProductId.Should().Be(2);
        cart.CartItems.Last().Quantity.Should().Be(3);
        _mockCartRepository.Verify(r => r.UpdateAsync(cart), Times.Once);
    }

    [Fact]
    public async Task AddItemToCartAsync_WithExistingProduct_IncreasesQuantity()
    {
        // Arrange
        var userId = "user123";
        var product = new Product { Id = 1, Name = "Mouse", Price = 25 };
        var existingItem = new CartItem { ProductId = 1, Quantity = 2, Product = product };
        var cart = new ShoppingCart
        {
            Id = 1,
            UserId = userId,
            CartItems = new List<CartItem> { existingItem }
        };

        _mockProductRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(product);
        _mockCartRepository.Setup(r => r.GetCartWithItemsAsync(userId))
            .ReturnsAsync(cart);
        _mockCartRepository.Setup(r => r.UpdateAsync(It.IsAny<ShoppingCart>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _shoppingCartService.AddItemToCartAsync(userId, 1, 3);

        // Assert
        result.Success.Should().BeTrue();
        existingItem.Quantity.Should().Be(5); // 2 + 3
        _mockCartRepository.Verify(r => r.UpdateAsync(cart), Times.Once);
    }

    [Fact]
    public async Task DecreaseItemQuantityAsync_WithNonExistentCart_ReturnsFailure()
    {
        // Arrange
        _mockCartRepository.Setup(r => r.GetCartWithItemsAsync("user123"))
            .ReturnsAsync((ShoppingCart?)null);

        // Act
        var result = await _shoppingCartService.DecreaseItemQuantityAsync("user123", 1, 1);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Cart not found");
    }

    [Fact]
    public async Task DecreaseItemQuantityAsync_WithNonExistentItem_ReturnsFailure()
    {
        // Arrange
        var cart = new ShoppingCart
        {
            Id = 1,
            UserId = "user123",
            CartItems = new List<CartItem>()
        };
        _mockCartRepository.Setup(r => r.GetCartWithItemsAsync("user123"))
            .ReturnsAsync(cart);

        // Act
        var result = await _shoppingCartService.DecreaseItemQuantityAsync("user123", 999, 1);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Item not found");
    }

    [Fact]
    public async Task DecreaseItemQuantityAsync_ReducesQuantityButKeepsItem()
    {
        // Arrange
        var item = new CartItem { ProductId = 1, Quantity = 5 };
        var cart = new ShoppingCart
        {
            Id = 1,
            UserId = "user123",
            CartItems = new List<CartItem> { item }
        };
        _mockCartRepository.Setup(r => r.GetCartWithItemsAsync("user123"))
            .ReturnsAsync(cart);
        _mockCartRepository.Setup(r => r.UpdateAsync(It.IsAny<ShoppingCart>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _shoppingCartService.DecreaseItemQuantityAsync("user123", 1, 2);

        // Assert
        result.Success.Should().BeTrue();
        item.Quantity.Should().Be(3); // 5 - 2
        cart.CartItems.Should().HaveCount(1);
        _mockCartRepository.Verify(r => r.UpdateAsync(cart), Times.Once);
    }

    [Fact]
    public async Task DecreaseItemQuantityAsync_WhenResultIsZeroOrLess_RemovesItem()
    {
        // Arrange
        var item = new CartItem { ProductId = 1, Quantity = 2 };
        var cart = new ShoppingCart
        {
            Id = 1,
            UserId = "user123",
            CartItems = new List<CartItem> { item }
        };
        _mockCartRepository.Setup(r => r.GetCartWithItemsAsync("user123"))
            .ReturnsAsync(cart);
        _mockCartRepository.Setup(r => r.UpdateAsync(It.IsAny<ShoppingCart>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _shoppingCartService.DecreaseItemQuantityAsync("user123", 1, 3);

        // Assert
        result.Success.Should().BeTrue();
        cart.CartItems.Should().BeEmpty();
        _mockCartRepository.Verify(r => r.UpdateAsync(cart), Times.Once);
    }

    [Fact]
    public async Task ClearCartAsync_WithExistingCart_ClearsAllItems()
    {
        // Arrange
        var cart = new ShoppingCart
        {
            Id = 1,
            UserId = "user123",
            CartItems = new List<CartItem>
            {
                new() { ProductId = 1, Quantity = 2 },
                new() { ProductId = 2, Quantity = 3 }
            }
        };
        _mockCartRepository.Setup(r => r.GetCartWithItemsAsync("user123"))
            .ReturnsAsync(cart);
        _mockCartRepository.Setup(r => r.UpdateAsync(It.IsAny<ShoppingCart>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _shoppingCartService.ClearCartAsync("user123");

        // Assert
        result.Should().BeTrue();
        cart.CartItems.Should().BeEmpty();
        _mockCartRepository.Verify(r => r.UpdateAsync(cart), Times.Once);
    }

    [Fact]
    public async Task ClearCartAsync_WithNonExistentCart_ReturnsFalse()
    {
        // Arrange
        _mockCartRepository.Setup(r => r.GetCartWithItemsAsync("nonexistent"))
            .ReturnsAsync((ShoppingCart?)null);

        // Act
        var result = await _shoppingCartService.ClearCartAsync("nonexistent");

        // Assert
        result.Should().BeFalse();
        _mockCartRepository.Verify(r => r.UpdateAsync(It.IsAny<ShoppingCart>()), Times.Never);
    }
}
