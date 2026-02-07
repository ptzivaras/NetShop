using Eshop.API.Services;
using Eshop.Contracts.DTOs;
using Eshop.Core.Models;
using Eshop.Core.Repositories;
using FluentAssertions;
using Moq;

namespace Eshop.UnitTests.Services;

public class StockAlertServiceTests
{
    private readonly Mock<IStockAlertRepository> _mockStockAlertRepository;
    private readonly StockAlertService _stockAlertService;

    public StockAlertServiceTests()
    {
        _mockStockAlertRepository = new Mock<IStockAlertRepository>();
        _stockAlertService = new StockAlertService(_mockStockAlertRepository.Object);
    }

    [Fact]
    public async Task GetAllAlertsAsync_ReturnsAllAlerts()
    {
        // Arrange
        var alerts = new List<StockAlert>
        {
            new()
            {
                Id = 1,
                ProductId = 1,
                ProductName = "Laptop",
                QuantityAtTrigger = 5,
                TriggeredAt = DateTime.UtcNow,
                IsAcknowledged = false
            },
            new()
            {
                Id = 2,
                ProductId = 2,
                ProductName = "Mouse",
                QuantityAtTrigger = 2,
                TriggeredAt = DateTime.UtcNow,
                IsAcknowledged = true
            }
        };
        _mockStockAlertRepository.Setup(r => r.GetAllAlertsAsync())
            .ReturnsAsync(alerts);

        // Act
        var result = await _stockAlertService.GetAllAlertsAsync();

        // Assert
        result.Should().HaveCount(2);
        result.First().ProductName.Should().Be("Laptop");
        result.Last().ProductName.Should().Be("Mouse");
        result.First().IsAcknowledged.Should().BeFalse();
        result.Last().IsAcknowledged.Should().BeTrue();
    }

    [Fact]
    public async Task GetUnacknowledgedCountAsync_ReturnsCorrectCount()
    {
        // Arrange
        _mockStockAlertRepository.Setup(r => r.GetUnacknowledgedCountAsync())
            .ReturnsAsync(5);

        // Act
        var result = await _stockAlertService.GetUnacknowledgedCountAsync();

        // Assert
        result.Should().Be(5);
    }

    [Fact]
    public async Task GetAlertByIdAsync_WithExistingId_ReturnsAlert()
    {
        // Arrange
        var alert = new StockAlert
        {
            Id = 1,
            ProductId = 1,
            ProductName = "Laptop",
            QuantityAtTrigger = 5,
            TriggeredAt = DateTime.UtcNow,
            IsAcknowledged = false
        };
        _mockStockAlertRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(alert);

        // Act
        var result = await _stockAlertService.GetAlertByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.ProductName.Should().Be("Laptop");
        result.QuantityAtTrigger.Should().Be(5);
        result.IsAcknowledged.Should().BeFalse();
    }

    [Fact]
    public async Task GetAlertByIdAsync_WithNonExistingId_ReturnsNull()
    {
        // Arrange
        _mockStockAlertRepository.Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((StockAlert?)null);

        // Act
        var result = await _stockAlertService.GetAlertByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task AcknowledgeAlertAsync_WithExistingAlert_AcknowledgesAndReturnsTrue()
    {
        // Arrange
        var alert = new StockAlert
        {
            Id = 1,
            ProductId = 1,
            ProductName = "Laptop",
            IsAcknowledged = false
        };
        _mockStockAlertRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(alert);
        _mockStockAlertRepository.Setup(r => r.UpdateAsync(It.IsAny<StockAlert>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _stockAlertService.AcknowledgeAlertAsync(1);

        // Assert
        result.Should().BeTrue();
        alert.IsAcknowledged.Should().BeTrue();
        _mockStockAlertRepository.Verify(r => r.UpdateAsync(alert), Times.Once);
    }

    [Fact]
    public async Task AcknowledgeAlertAsync_WithNonExistingAlert_ReturnsFalse()
    {
        // Arrange
        _mockStockAlertRepository.Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((StockAlert?)null);

        // Act
        var result = await _stockAlertService.AcknowledgeAlertAsync(999);

        // Assert
        result.Should().BeFalse();
        _mockStockAlertRepository.Verify(r => r.UpdateAsync(It.IsAny<StockAlert>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAlertAsync_WithExistingAlert_DeletesAndReturnsTrue()
    {
        // Arrange
        var alert = new StockAlert { Id = 1, ProductName = "To Delete" };
        _mockStockAlertRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(alert);
        _mockStockAlertRepository.Setup(r => r.DeleteAsync(It.IsAny<StockAlert>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _stockAlertService.DeleteAlertAsync(1);

        // Assert
        result.Should().BeTrue();
        _mockStockAlertRepository.Verify(r => r.DeleteAsync(alert), Times.Once);
    }

    [Fact]
    public async Task DeleteAlertAsync_WithNonExistingAlert_ReturnsFalse()
    {
        // Arrange
        _mockStockAlertRepository.Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((StockAlert?)null);

        // Act
        var result = await _stockAlertService.DeleteAlertAsync(999);

        // Assert
        result.Should().BeFalse();
        _mockStockAlertRepository.Verify(r => r.DeleteAsync(It.IsAny<StockAlert>()), Times.Never);
    }
}
