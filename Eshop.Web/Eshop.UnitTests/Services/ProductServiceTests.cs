using Eshop.API.Services;
using Eshop.Contracts.DTOs;
using Eshop.Core.Models;
using Eshop.Core.Repositories;
using FluentAssertions;
using Moq;

namespace Eshop.UnitTests.Services;

public class ProductServiceTests
{
    private readonly Mock<IProductRepository> _mockProductRepository;
    private readonly ProductService _productService;

    public ProductServiceTests()
    {
        _mockProductRepository = new Mock<IProductRepository>();
        _productService = new ProductService(_mockProductRepository.Object);
    }

    [Fact]
    public async Task GetProductsAsync_WithValidPagination_ReturnsPagedProducts()
    {
        // Arrange
        var products = new List<Product>
        {
            new() { Id = 1, Name = "Product 1", Price = 10, StockQuantity = 5, CategoryId = 1 },
            new() { Id = 2, Name = "Product 2", Price = 20, StockQuantity = 10, CategoryId = 1 }
        };
        _mockProductRepository.Setup(r => r.GetPagedAsync(1, 10, null, null))
            .ReturnsAsync((products, 2));

        // Act
        var result = await _productService.GetProductsAsync(1, 10, null, null);

        // Assert
        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(10);
        result.Items.First().Name.Should().Be("Product 1");
    }

    [Theory]
    [InlineData(0, 10, 1, 10)] // Invalid page, should default to 1
    [InlineData(-5, 10, 1, 10)] // Negative page, should default to 1
    [InlineData(1, 0, 1, 11)] // Invalid pageSize, should default to 11
    [InlineData(1, 200, 1, 11)] // pageSize > 100, should default to 11
    public async Task GetProductsAsync_WithInvalidPagination_UsesDefaultValues(
        int inputPage, int inputPageSize, int expectedPage, int expectedPageSize)
    {
        // Arrange
        _mockProductRepository.Setup(r => r.GetPagedAsync(It.IsAny<int>(), It.IsAny<int>(), null, null))
            .ReturnsAsync((new List<Product>(), 0));

        // Act
        var result = await _productService.GetProductsAsync(inputPage, inputPageSize, null, null);

        // Assert
        result.Page.Should().Be(expectedPage);
        result.PageSize.Should().Be(expectedPageSize);
    }

    [Fact]
    public async Task GetProductsAsync_WithSearchTerm_FiltersProducts()
    {
        // Arrange
        var filteredProducts = new List<Product>
        {
            new() { Id = 1, Name = "Laptop", Price = 1000, StockQuantity = 3, CategoryId = 1 }
        };
        _mockProductRepository.Setup(r => r.GetPagedAsync(1, 10, "Laptop", null))
            .ReturnsAsync((filteredProducts, 1));

        // Act
        var result = await _productService.GetProductsAsync(1, 10, "Laptop", null);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items.First().Name.Should().Be("Laptop");
        result.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task GetProductsAsync_WithCategoryFilter_ReturnsOnlyCategoryProducts()
    {
        // Arrange
        var categoryProducts = new List<Product>
        {
            new() { Id = 1, Name = "Product 1", Price = 10, StockQuantity = 5, CategoryId = 2 },
            new() { Id = 2, Name = "Product 2", Price = 20, StockQuantity = 10, CategoryId = 2 }
        };
        _mockProductRepository.Setup(r => r.GetPagedAsync(1, 10, null, 2))
            .ReturnsAsync((categoryProducts, 2));

        // Act
        var result = await _productService.GetProductsAsync(1, 10, null, 2);

        // Assert
        result.Items.Should().HaveCount(2);
        result.Items.Should().OnlyContain(p => p.CategoryId == 2);
    }

    [Fact]
    public async Task GetProductByIdAsync_WithExistingId_ReturnsProduct()
    {
        // Arrange
        var product = new Product
        {
            Id = 1,
            Name = "Test Product",
            Description = "Test Description",
            Price = 50,
            StockQuantity = 10,
            CategoryId = 1
        };
        _mockProductRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(product);

        // Act
        var result = await _productService.GetProductByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.Name.Should().Be("Test Product");
        result.Price.Should().Be(50);
    }

    [Fact]
    public async Task GetProductByIdAsync_WithNonExistingId_ReturnsNull()
    {
        // Arrange
        _mockProductRepository.Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((Product?)null);

        // Act
        var result = await _productService.GetProductByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateProductAsync_WithValidDto_CreatesProduct()
    {
        // Arrange
        var productDto = new ProductDto
        {
            Name = "New Product",
            Description = "New Description",
            Price = 100,
            StockQuantity = 20,
            CategoryId = 1
        };
        var createdProduct = new Product
        {
            Id = 10,
            Name = productDto.Name,
            Description = productDto.Description,
            Price = productDto.Price,
            StockQuantity = productDto.StockQuantity,
            CategoryId = productDto.CategoryId
        };
        _mockProductRepository.Setup(r => r.AddAsync(It.IsAny<Product>()))
            .ReturnsAsync(createdProduct);

        // Act
        var result = await _productService.CreateProductAsync(productDto);

        // Assert
        result.Id.Should().Be(10);
        result.Name.Should().Be("New Product");
        _mockProductRepository.Verify(r => r.AddAsync(It.Is<Product>(p =>
            p.Name == "New Product" &&
            p.Price == 100 &&
            p.StockQuantity == 20
        )), Times.Once);
    }

    [Fact]
    public async Task UpdateProductAsync_WithExistingProduct_UpdatesAndReturnsTrue()
    {
        // Arrange
        var existingProduct = new Product
        {
            Id = 1,
            Name = "Old Name",
            Description = "Old Description",
            Price = 50,
            StockQuantity = 10,
            CategoryId = 1
        };
        var updateDto = new ProductDto
        {
            Id = 1,
            Name = "Updated Name",
            Description = "Updated Description",
            Price = 75,
            StockQuantity = 15,
            CategoryId = 2
        };
        _mockProductRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(existingProduct);
        _mockProductRepository.Setup(r => r.UpdateAsync(It.IsAny<Product>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _productService.UpdateProductAsync(1, updateDto);

        // Assert
        result.Should().BeTrue();
        existingProduct.Name.Should().Be("Updated Name");
        existingProduct.Price.Should().Be(75);
        existingProduct.StockQuantity.Should().Be(15);
        _mockProductRepository.Verify(r => r.UpdateAsync(existingProduct), Times.Once);
    }

    [Fact]
    public async Task UpdateProductAsync_WithNonExistingProduct_ReturnsFalse()
    {
        // Arrange
        var updateDto = new ProductDto { Id = 999, Name = "Non-existent" };
        _mockProductRepository.Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((Product?)null);

        // Act
        var result = await _productService.UpdateProductAsync(999, updateDto);

        // Assert
        result.Should().BeFalse();
        _mockProductRepository.Verify(r => r.UpdateAsync(It.IsAny<Product>()), Times.Never);
    }

    [Fact]
    public async Task DeleteProductAsync_WithExistingProduct_DeletesAndReturnsTrue()
    {
        // Arrange
        var product = new Product { Id = 1, Name = "To Delete" };
        _mockProductRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(product);
        _mockProductRepository.Setup(r => r.DeleteAsync(It.IsAny<Product>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _productService.DeleteProductAsync(1);

        // Assert
        result.Should().BeTrue();
        _mockProductRepository.Verify(r => r.DeleteAsync(product), Times.Once);
    }

    [Fact]
    public async Task DeleteProductAsync_WithNonExistingProduct_ReturnsFalse()
    {
        // Arrange
        _mockProductRepository.Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((Product?)null);

        // Act
        var result = await _productService.DeleteProductAsync(999);

        // Assert
        result.Should().BeFalse();
        _mockProductRepository.Verify(r => r.DeleteAsync(It.IsAny<Product>()), Times.Never);
    }

    [Fact]
    public async Task GetProductImageAsync_WithProductHavingImage_ReturnsImageBytes()
    {
        // Arrange
        var imageBytes = new byte[] { 0x01, 0x02, 0x03 };
        var product = new Product
        {
            Id = 1,
            Name = "Product with Image",
            ImageBytes = imageBytes,
            ImageContentType = "image/png"
        };
        _mockProductRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(product);

        // Act
        var result = await _productService.GetProductImageAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Value.ImageBytes.Should().BeEquivalentTo(imageBytes);
        result.Value.ContentType.Should().Be("image/png");
    }

    [Fact]
    public async Task GetProductImageAsync_WithProductWithoutImage_ReturnsNull()
    {
        // Arrange
        var product = new Product { Id = 1, Name = "Product without Image" };
        _mockProductRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(product);

        // Act
        var result = await _productService.GetProductImageAsync(1);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateProductImageAsync_WithExistingProduct_UpdatesImage()
    {
        // Arrange
        var product = new Product { Id = 1, Name = "Product" };
        var imageBytes = new byte[] { 0x01, 0x02, 0x03 };
        _mockProductRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(product);
        _mockProductRepository.Setup(r => r.UpdateAsync(It.IsAny<Product>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _productService.UpdateProductImageAsync(1, imageBytes, "image/jpeg", "test.jpg", 1024);

        // Assert
        result.Should().BeTrue();
        product.ImageBytes.Should().BeEquivalentTo(imageBytes);
        product.ImageContentType.Should().Be("image/jpeg");
        product.ImageFileName.Should().Be("test.jpg");
        product.ImageSize.Should().Be(1024);
        _mockProductRepository.Verify(r => r.UpdateAsync(product), Times.Once);
    }
}
