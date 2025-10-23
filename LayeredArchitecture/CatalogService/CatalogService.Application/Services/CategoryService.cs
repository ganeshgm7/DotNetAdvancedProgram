using CatalogService.Application.DTOs;
using CatalogService.Application.Interfaces;
using CatalogService.Domain.Entities;
using CatalogService.Domain.Interfaces;

namespace CatalogService.Application.Services;

public class CategoryService(ICategoryRepository categoryRepository) : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository = categoryRepository;

    public async Task<CategoryDto?> GetByIdAsync(int id)
    {
        Category? category = await _categoryRepository.GetByIdAsync(id);
        return category == null ? null : MapToDto(category);
    }

    public async Task<IEnumerable<CategoryDto>> GetAllAsync()
    {
        IEnumerable<Category> categories = await _categoryRepository.GetAllAsync();
        return categories.Select(MapToDto);
    }

    public async Task AddAsync(CategoryDto categoryDto)
    {
        Category category = MapToEntity(categoryDto);
        await _categoryRepository.AddAsync(category);
    }

    public async Task UpdateAsync(CategoryDto categoryDto)
    {
        Category category = MapToEntity(categoryDto);
        await _categoryRepository.UpdateAsync(category);
    }

    public async Task DeleteAsync(int id)
    {
        await _categoryRepository.DeleteAsync(id);
    }

    // Mapping helpers
    private CategoryDto MapToDto(Category category) =>
        new()
        {
            Id = category.Id,
            Name = category.Name,
            ImageUrl = category.ImageUrl,
            ParentCategoryId = category.ParentCategoryId
        };

    private static Category MapToEntity(CategoryDto dto) =>
        new()
        {
            Id = dto.Id,
            Name = dto.Name,
            ImageUrl = dto.ImageUrl,
            ParentCategoryId = dto.ParentCategoryId == 0 ? null : dto.ParentCategoryId
        };
}
