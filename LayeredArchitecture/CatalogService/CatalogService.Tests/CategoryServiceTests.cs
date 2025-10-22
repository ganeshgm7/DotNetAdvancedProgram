using CatalogService.Application.DTOs;
using CatalogService.Application.Services;
using CatalogService.Domain.Entities;
using CatalogService.Domain.Interfaces;
using Moq;

namespace CatalogService.Tests;

public class CategoryServiceTests
{
    [Fact]
    public async Task GetByIdAsync_ReturnsCategoryDto_WhenCategoryExists()
    {
        // Arrange
        Mock<ICategoryRepository> mockRepo = new();
        Category category = new()
        {
            Id = 1,
            Name = "Test"
        };

        mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(category);
        CategoryService service = new(mockRepo.Object);

        // Act
        CategoryDto? result = await service.GetByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("Test", result.Name);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenCategoryDoesNotExist()
    {
        // Arrange
        Mock<ICategoryRepository> mockRepo = new();
        mockRepo.Setup(r => r.GetByIdAsync(2)).ReturnsAsync((Category)null);
        CategoryService service = new(mockRepo.Object);

        // Act
        CategoryDto? result = await service.GetByIdAsync(2);

        // Assert
        Assert.Null(result);
    }
}
