using CatalogService.Application.DTOs;
using CatalogService.Application.Interfaces;
using CatalogService.Application.Services;
using CatalogService.Domain.Entities;
using CatalogService.Domain.Interfaces;
using Moq;

namespace CatalogService.Tests;

public class ProductServiceTests
{
    [Fact]
    public async Task GetByIdAsync_ReturnsProductDto_WhenProductExists()
    {
        // Arrange
        Mock<IProductRepository> mockRepo = new();
        Mock<IProductEventPublisher> mockEventPublisher = new();
        Product product = new() { Id = 1, Name = "Phone", CategoryId = 1, Price = 100, Amount = 5 };
        mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(product);
        ProductService service = new(mockRepo.Object, mockEventPublisher.Object);

        // Act
        ProductDto? result = await service.GetByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("Phone", result.Name);
    }
}
