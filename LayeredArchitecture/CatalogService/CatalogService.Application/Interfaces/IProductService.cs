using CatalogService.Application.DTOs;

namespace CatalogService.Application.Interfaces;

public interface IProductService
{
    Task<ProductDto?> GetByIdAsync(int id);

    Task<IEnumerable<ProductDto>> GetAllAsync(int? categoryId = null, int page = 1, int pageSize = 10);

    Task<ProductDto> AddAsync(ProductDto product);

    Task UpdateAsync(ProductDto product);

    Task DeleteAsync(int id);

    Task DeleteByCategoryIdAsync(int categoryId);
}