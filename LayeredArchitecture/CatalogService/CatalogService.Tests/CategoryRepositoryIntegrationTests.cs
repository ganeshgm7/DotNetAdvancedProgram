using CatalogService.Domain.Entities;
using CatalogService.Infrastructure.Data;
using CatalogService.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.Tests;

public class CategoryRepositoryIntegrationTests
{
    [Fact]
    public async Task AddAndGetCategory_WorksCorrectly()
    {
        // Arrange
        CatalogDbContext context = GetInMemoryDbContext();
        CategoryRepository repo = new(context);
        Category category = new()
        {
            Name = "Electronics",
            ImageUrl = "https://example.com/electronics.jpg",
            ParentCategoryId = null
        };

        // Act
        await repo.AddAsync(category);
        Category? result = await repo.GetByIdAsync(category.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Electronics", result.Name);
    }

    private static CatalogDbContext GetInMemoryDbContext()
    {
        DbContextOptions<CatalogDbContext> options = new DbContextOptionsBuilder<CatalogDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb")
            .Options;

        return new CatalogDbContext(options);
    }
}

