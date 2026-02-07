using Eshop.API.Services;
using Eshop.Core.Models;
using Eshop.Core.Repositories;
using FluentAssertions;
using Moq;

namespace Eshop.UnitTests.Services;

public class OrderServiceTests
{
    private readonly Mock<IOrderRepository> _mockOrderRepository;
    private readonly Mock<IShoppingCartRepository> _mockCartRepository;
    private readonly Mock<IProductRepository> _mockProductRepository;
    private readonly OrderService _orderService;

    public OrderServiceTests()
    {
        _mockOrderRepository = new Mock<IOrderRepository>();
        _mockCartRepository = new Mock<IShoppingCartRepository>();
        _mockProductRepository = new Mock<IProductRepository>();
        _orderService = new OrderService(
            _mockOrderRepository.Object,
            _mockCartRepository.Object,
            _mockProductRepository.Object);
    }

    [Fact]
    public async Task CreateOrderAsync_WithEmptyCart_ReturnsFailure()
    {
        // Arrange
        var userId = "user123";
        _mockCartRepository.Setup(r => r.GetCartWithItemsAsync(userId))
            .ReturnsAsync((ShoppingCart?)null);

        // Act
        var result = await _orderService.CreateOrderAsync(userId);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Cart is empty");
        result.OrderId.Should().BeNull();
    }

    [Fact]
    public async Task CreateOrderAsync_WithCartHavingNoItems_ReturnsFailure()
    {
        // Arrange
        var userId = "user123";
        var emptyCart = new ShoppingCart
        {
            Id = 1,
            UserId = userId,
            CartItems = new List<CartItem>()
        };
        _mockCartRepository.Setup(r => r.GetCartWithItemsAsync(userId))
            .ReturnsAsync(emptyCart);

        // Act
        var result = await _orderService.CreateOrderAsync(userId);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Cart is empty");
    }

    [Fact]
    public async Task CreateOrderAsync_WithInsufficientStock_ReturnsFailure()
    {
        // Arrange
        var userId = "user123";
        var product = new Product
        {
            Id = 1,
            Name = "Low Stock Product",
            Price = 100,
            StockQuantity = 2 // Only 2 in stock
        };
        var cart = new ShoppingCart
        {
            Id = 1,
            UserId = userId,
            CartItems = new List<CartItem>
            {
                new()
                {
                    Id = 1,
                    ProductId = 1,
                    Quantity = 5, // Trying to order 5
                    Product = product
                }
            }
        };
        _mockCartRepository.Setup(r => r.GetCartWithItemsAsync(userId))
            .ReturnsAsync(cart);

        // Act
        var result = await _orderService.CreateOrderAsync(userId);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Not enough stock");
        result.OrderId.Should().BeNull();
    }

    [Fact]
    public async Task CreateOrderAsync_WithInvalidQuantity_ReturnsFailure()
    {
        // Arrange
        var userId = "user123";
        var product = new Product { Id = 1, Name = "Product", Price = 100, StockQuantity = 10 };
        var cart = new ShoppingCart
        {
            Id = 1,
            UserId = userId,
            CartItems = new List<CartItem>
            {
                new()
                {
                    Id = 1,
                    ProductId = 1,
                    Quantity = 0, // Invalid quantity
                    Product = product
                }
            }
        };
        _mockCartRepository.Setup(r => r.GetCartWithItemsAsync(userId))
            .ReturnsAsync(cart);

        // Act
        var result = await _orderService.CreateOrderAsync(userId);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Invalid quantity");
    }

    [Fact]
    public async Task CreateOrderAsync_WithMissingProduct_ReturnsFailure()
    {
        // Arrange
        var userId = "user123";
        var cart = new ShoppingCart
        {
            Id = 1,
            UserId = userId,
            CartItems = new List<CartItem>
            {
                new()
                {
                    Id = 1,
                    ProductId = 999,
                    Quantity = 1,
                    Product = null // Product not found
                }
            }
        };
        _mockCartRepository.Setup(r => r.GetCartWithItemsAsync(userId))
            .ReturnsAsync(cart);

        // Act
        var result = await _orderService.CreateOrderAsync(userId);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Product 999 not found");
    }

    [Fact]
    public async Task CreateOrderAsync_WithValidCart_CreatesOrderSuccessfully()
    {
        // Arrange
        var userId = "user123";
        var product1 = new Product { Id = 1, Name = "Product 1", Price = 50, StockQuantity = 10 };
        var product2 = new Product { Id = 2, Name = "Product 2", Price = 30, StockQuantity = 5 };
        
        var cart = new ShoppingCart
        {
            Id = 1,
            UserId = userId,
            CartItems = new List<CartItem>
            {
                new() { Id = 1, ProductId = 1, Quantity = 2, Product = product1 },
                new() { Id = 2, ProductId = 2, Quantity = 1, Product = product2 }
            }
        };

        var createdOrder = new Order
        {
            Id = 10,
            UserId = userId,
            OrderDate = DateTime.UtcNow,
            TotalPrice = 130 // (50*2) + (30*1)
        };

        _mockCartRepository.Setup(r => r.GetCartWithItemsAsync(userId))
            .ReturnsAsync(cart);
        _mockProductRepository.Setup(r => r.UpdateAsync(It.IsAny<Product>()))
            .Returns(Task.CompletedTask);
        _mockOrderRepository.Setup(r => r.AddAsync(It.IsAny<Order>()))
            .ReturnsAsync(createdOrder);
        _mockCartRepository.Setup(r => r.UpdateAsync(It.IsAny<ShoppingCart>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _orderService.CreateOrderAsync(userId);

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Contain("successfully");
        result.OrderId.Should().Be(10);

        // Verify stock was updated
        product1.StockQuantity.Should().Be(8); // 10 - 2
        product2.StockQuantity.Should().Be(4); // 5 - 1

        // Verify repositories were called
        _mockProductRepository.Verify(r => r.UpdateAsync(product1), Times.Once);
        _mockProductRepository.Verify(r => r.UpdateAsync(product2), Times.Once);
        _mockOrderRepository.Verify(r => r.AddAsync(It.Is<Order>(o =>
            o.UserId == userId &&
            o.OrderItems.Count == 2 &&
            o.TotalPrice == 130
        )), Times.Once);
        _mockCartRepository.Verify(r => r.UpdateAsync(cart), Times.Once);
    }

    [Fact]
    public async Task CreateOrderAsync_WithNullUserId_ReturnsFailure()
    {
        // Arrange
        string userId = null!;

        // Act
        var result = await _orderService.CreateOrderAsync(userId);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Invalid user ID");
    }

    [Fact]
    public async Task CreateOrderAsync_WithEmptyUserId_ReturnsFailure()
    {
        // Arrange
        var userId = "";

        // Act
        var result = await _orderService.CreateOrderAsync(userId);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Invalid user ID");
    }

    [Fact]
    public async Task GetAllOrdersAsync_ReturnsAllOrders()
    {
        // Arrange
        var orders = new List<Order>
        {
            new()
            {
                Id = 1,
                UserId = "user1",
                OrderDate = DateTime.UtcNow,
                TotalPrice = 100,
                OrderItems = new List<OrderItem>()
            },
            new()
            {
                Id = 2,
                UserId = "user2",
                OrderDate = DateTime.UtcNow,
                TotalPrice = 200,
                OrderItems = new List<OrderItem>()
            }
        };
        _mockOrderRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(orders);

        // Act
        var result = await _orderService.GetAllOrdersAsync();

        // Assert
        result.Should().HaveCount(2);
        result.First().Id.Should().Be(1);
        result.Last().Id.Should().Be(2);
    }

    [Fact]
    public async Task GetOrdersByUserIdAsync_ReturnsPaginatedOrders()
    {
        // Arrange
        var userId = "user123";
        var orders = new List<Order>
        {
            new()
            {
                Id = 1,
                UserId = userId,
                OrderDate = DateTime.UtcNow,
                TotalPrice = 100,
                OrderItems = new List<OrderItem>()
            }
        };
        _mockOrderRepository.Setup(r => r.GetOrdersByUserIdAsync(userId, 1, 10))
            .ReturnsAsync(orders);
        _mockOrderRepository.Setup(r => r.CountByUserIdAsync(userId))
            .ReturnsAsync(15); // Total of 15 orders

        // Act
        var result = await _orderService.GetOrdersByUserIdAsync(userId, 1, 10);

        // Assert
        result.Orders.Should().HaveCount(1);
        result.TotalOrders.Should().Be(15);
        result.Orders.First().UserId.Should().Be(userId);
    }

    [Fact]
    public async Task GetOrderByIdAsync_WithExistingOrder_ReturnsOrderDto()
    {
        // Arrange
        var order = new Order
        {
            Id = 1,
            UserId = "user123",
            OrderDate = DateTime.UtcNow,
            TotalPrice = 150,
            OrderItems = new List<OrderItem>
            {
                new()
                {
                    ProductId = 1,
                    Quantity = 2,
                    UnitPrice = 50,
                    Product = new Product { Id = 1, Name = "Test Product" }
                }
            }
        };
        _mockOrderRepository.Setup(r => r.GetOrderWithItemsAsync(1))
            .ReturnsAsync(order);

        // Act
        var result = await _orderService.GetOrderByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.UserId.Should().Be("user123");
        result.TotalPrice.Should().Be(150);
        result.Items.Should().HaveCount(1);
        result.Items.First().ProductName.Should().Be("Test Product");
    }

    [Fact]
    public async Task GetOrderByIdAsync_WithNonExistingOrder_ReturnsNull()
    {
        // Arrange
        _mockOrderRepository.Setup(r => r.GetOrderWithItemsAsync(999))
            .ReturnsAsync((Order?)null);

        // Act
        var result = await _orderService.GetOrderByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }
}
