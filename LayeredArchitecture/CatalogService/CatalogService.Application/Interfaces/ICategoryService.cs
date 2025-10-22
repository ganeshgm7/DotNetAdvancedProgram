using CatalogService.Application.DTOs;

namespace CatalogService.Application.Interfaces;

public interface ICategoryService
{
    Task<CategoryDto?> GetByIdAsync(int id);
    Task<IEnumerable<CategoryDto>> GetAllAsync();
    Task AddAsync(CategoryDto category);
    Task UpdateAsync(CategoryDto category);
    Task DeleteAsync(int id);
}
