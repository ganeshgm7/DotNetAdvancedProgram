using CatalogService.Application.DTOs;

namespace CatalogService.Application.Interfaces;

public interface IProductService
{
    Task<ProductDto?> GetByIdAsync(int id);
    Task<IEnumerable<ProductDto>> GetAllAsync();
    Task AddAsync(ProductDto product);
    Task UpdateAsync(ProductDto product);
    Task DeleteAsync(int id);
}
