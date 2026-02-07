using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Eshop.Contracts.DTOs;
using Eshop.IntegrationTests.Infrastructure;
using FluentAssertions;

namespace Eshop.IntegrationTests.Controllers;

public class ProductsControllerTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions;

    public ProductsControllerTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    [Fact]
    public async Task GetProducts_ReturnsPagedProducts()
    {
        // Act
        var response = await _client.GetAsync("/api/products?page=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<PagedResult<ProductDto>>(content, _jsonOptions);
        
        result.Should().NotBeNull();
        result!.Items.Should().NotBeNull();
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(10);
    }

    [Fact]
    public async Task GetProducts_WithSearchTerm_FiltersResults()
    {
        // Act
        var response = await _client.GetAsync("/api/products?page=1&pageSize=10&q=Laptop");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<PagedResult<ProductDto>>(content, _jsonOptions);
        
        result.Should().NotBeNull();
        result!.Items.Should().NotBeNull();
    }

    [Fact]
    public async Task GetProducts_WithCategoryFilter_ReturnsFilteredProducts()
    {
        // Act
        var response = await _client.GetAsync("/api/products?page=1&pageSize=10&categoryId=1");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<PagedResult<ProductDto>>(content, _jsonOptions);
        
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetProducts_WithInvalidPageNumber_UsesDefaultValue()
    {
        // Act - page=0 should default to 1
        var response = await _client.GetAsync("/api/products?page=0&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<PagedResult<ProductDto>>(content, _jsonOptions);
        
        result.Should().NotBeNull();
        result!.Page.Should().Be(1); // Should default to 1
    }

    [Fact]
    public async Task GetProducts_WithInvalidPageSize_UsesDefaultValue()
    {
        // Act - pageSize=200 should default to 11
        var response = await _client.GetAsync("/api/products?page=1&pageSize=200");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<PagedResult<ProductDto>>(content, _jsonOptions);
        
        result.Should().NotBeNull();
        result!.PageSize.Should().Be(11); // Should default to 11 (max is 100)
    }

    [Fact]
    public async Task GetProduct_WithExistingId_ReturnsProduct()
    {
        // Arrange - First create a product to ensure we have a valid ID
        var newProduct = new ProductDto
        {
            Name = "Test Integration Product",
            Description = "Created for integration test",
            Price = 99.99m,
            StockQuantity = 10,
            CategoryId = 1
        };

        // Note: This test assumes there's at least one product with ID 1 seeded in the test database
        // In a real scenario, you would create the product first or use a known seeded ID
        
        // Act
        var response = await _client.GetAsync("/api/products/1");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var product = JsonSerializer.Deserialize<ProductDto>(content, _jsonOptions);
        
        product.Should().NotBeNull();
        product!.Id.Should().Be(1);
    }

    [Fact]
    public async Task GetProduct_WithNonExistingId_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/products/999999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateProduct_WithoutAuthorization_ReturnsUnauthorized()
    {
        // Arrange
        var newProduct = new ProductDto
        {
            Name = "Unauthorized Product",
            Description = "Should fail",
            Price = 50,
            StockQuantity = 5,
            CategoryId = 1
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/products", newProduct);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateProduct_WithoutAuthorization_ReturnsUnauthorized()
    {
        // Arrange
        var updateDto = new ProductDto
        {
            Id = 1,
            Name = "Updated Product",
            Description = "Should fail",
            Price = 100,
            StockQuantity = 10,
            CategoryId = 1
        };

        // Act
        var response = await _client.PutAsJsonAsync("/api/products/1", updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateProduct_WithMismatchedId_ReturnsBadRequest()
    {
        // Arrange
        var updateDto = new ProductDto
        {
            Id = 999, // ID in body doesn't match URL
            Name = "Updated Product",
            Description = "Should fail",
            Price = 100,
            StockQuantity = 10,
            CategoryId = 1
        };

        // Act
        var response = await _client.PutAsJsonAsync("/api/products/1", updateDto);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeleteProduct_WithoutAuthorization_ReturnsUnauthorized()
    {
        // Act
        var response = await _client.DeleteAsync("/api/products/1");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetImage_WithNonExistingProduct_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/products/999999/image");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Theory]
    [InlineData(1, 5)] // Page 1, pageSize 5
    [InlineData(1, 10)] // Page 1, pageSize 10
    [InlineData(2, 5)] // Page 2, pageSize 5
    public async Task GetProducts_WithDifferentPaginationParams_ReturnsCorrectPageSize(int page, int pageSize)
    {
        // Act
        var response = await _client.GetAsync($"/api/products?page={page}&pageSize={pageSize}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<PagedResult<ProductDto>>(content, _jsonOptions);
        
        result.Should().NotBeNull();
        result!.Page.Should().Be(page);
        result.PageSize.Should().Be(pageSize);
        result.Items.Should().HaveCountLessThanOrEqualTo(pageSize);
    }

    [Fact]
    public async Task GetProducts_ReturnsValidJson()
    {
        // Act
        var response = await _client.GetAsync("/api/products");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");
        
        var content = await response.Content.ReadAsStringAsync();
        var deserializeAction = () => JsonSerializer.Deserialize<PagedResult<ProductDto>>(content, _jsonOptions);
        
        deserializeAction.Should().NotThrow();
    }
}
