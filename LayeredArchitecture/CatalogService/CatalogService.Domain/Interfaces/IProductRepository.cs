using CatalogService.Domain.Entities;

namespace CatalogService.Domain.Interfaces;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(int id);

    Task<IEnumerable<Product>> GetAllAsync();

    Task AddAsync(Product product);

    Task UpdateAsync(Product product);

    Task DeleteAsync(int id);

    Task DeleteByCategoryIdAsync(int categoryId);
}