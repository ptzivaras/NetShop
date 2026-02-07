using Eshop.API.Services;
using Eshop.Contracts.DTOs;
using Eshop.Core.Models;
using Eshop.Core.Repositories;
using FluentAssertions;
using Moq;

namespace Eshop.UnitTests.Services;

public class CategoryServiceTests
{
    private readonly Mock<ICategoryRepository> _mockCategoryRepository;
    private readonly CategoryService _categoryService;

    public CategoryServiceTests()
    {
        _mockCategoryRepository = new Mock<ICategoryRepository>();
        _categoryService = new CategoryService(_mockCategoryRepository.Object);
    }

    [Fact]
    public async Task GetAllCategoriesAsync_ReturnsAllCategories()
    {
        // Arrange
        var categories = new List<Category>
        {
            new() { Id = 1, Name = "Electronics", Description = "Electronic items" },
            new() { Id = 2, Name = "Books", Description = "All books" }
        };
        _mockCategoryRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(categories);

        // Act
        var result = await _categoryService.GetAllCategoriesAsync();

        // Assert
        result.Should().HaveCount(2);
        result.First().Name.Should().Be("Electronics");
        result.Last().Name.Should().Be("Books");
    }

    [Fact]
    public async Task GetCategoryByIdAsync_WithExistingId_ReturnsCategory()
    {
        // Arrange
        var category = new Category
        {
            Id = 1,
            Name = "Electronics",
            Description = "Electronic devices"
        };
        _mockCategoryRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(category);

        // Act
        var result = await _categoryService.GetCategoryByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.Name.Should().Be("Electronics");
        result.Description.Should().Be("Electronic devices");
    }

    [Fact]
    public async Task GetCategoryByIdAsync_WithNonExistingId_ReturnsNull()
    {
        // Arrange
        _mockCategoryRepository.Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((Category?)null);

        // Act
        var result = await _categoryService.GetCategoryByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateCategoryAsync_CreatesNewCategory()
    {
        // Arrange
        var categoryDto = new CategoryDto
        {
            Name = "New Category",
            Description = "New Description"
        };
        var createdCategory = new Category
        {
            Id = 5,
            Name = categoryDto.Name,
            Description = categoryDto.Description
        };
        _mockCategoryRepository.Setup(r => r.AddAsync(It.IsAny<Category>()))
            .ReturnsAsync(createdCategory);

        // Act
        var result = await _categoryService.CreateCategoryAsync(categoryDto);

        // Assert
        result.Id.Should().Be(5);
        result.Name.Should().Be("New Category");
        result.Description.Should().Be("New Description");
        _mockCategoryRepository.Verify(r => r.AddAsync(It.Is<Category>(c =>
            c.Name == "New Category" &&
            c.Description == "New Description"
        )), Times.Once);
    }

    [Fact]
    public async Task UpdateCategoryAsync_WithExistingCategory_UpdatesAndReturnsTrue()
    {
        // Arrange
        var existingCategory = new Category
        {
            Id = 1,
            Name = "Old Name",
            Description = "Old Description"
        };
        var updateDto = new CategoryDto
        {
            Id = 1,
            Name = "Updated Name",
            Description = "Updated Description"
        };
        _mockCategoryRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(existingCategory);
        _mockCategoryRepository.Setup(r => r.UpdateAsync(It.IsAny<Category>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _categoryService.UpdateCategoryAsync(1, updateDto);

        // Assert
        result.Should().BeTrue();
        existingCategory.Name.Should().Be("Updated Name");
        existingCategory.Description.Should().Be("Updated Description");
        _mockCategoryRepository.Verify(r => r.UpdateAsync(existingCategory), Times.Once);
    }

    [Fact]
    public async Task UpdateCategoryAsync_WithNonExistingCategory_ReturnsFalse()
    {
        // Arrange
        var updateDto = new CategoryDto { Id = 999, Name = "Non-existent" };
        _mockCategoryRepository.Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((Category?)null);

        // Act
        var result = await _categoryService.UpdateCategoryAsync(999, updateDto);

        // Assert
        result.Should().BeFalse();
        _mockCategoryRepository.Verify(r => r.UpdateAsync(It.IsAny<Category>()), Times.Never);
    }

    [Fact]
    public async Task DeleteCategoryAsync_WithExistingCategory_DeletesAndReturnsTrue()
    {
        // Arrange
        var category = new Category { Id = 1, Name = "To Delete" };
        _mockCategoryRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(category);
        _mockCategoryRepository.Setup(r => r.DeleteAsync(It.IsAny<Category>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _categoryService.DeleteCategoryAsync(1);

        // Assert
        result.Should().BeTrue();
        _mockCategoryRepository.Verify(r => r.DeleteAsync(category), Times.Once);
    }

    [Fact]
    public async Task DeleteCategoryAsync_WithNonExistingCategory_ReturnsFalse()
    {
        // Arrange
        _mockCategoryRepository.Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((Category?)null);

        // Act
        var result = await _categoryService.DeleteCategoryAsync(999);

        // Assert
        result.Should().BeFalse();
        _mockCategoryRepository.Verify(r => r.DeleteAsync(It.IsAny<Category>()), Times.Never);
    }
}
