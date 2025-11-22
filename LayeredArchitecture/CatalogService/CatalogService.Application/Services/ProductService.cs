using CatalogService.Application.DTOs;
using CatalogService.Application.Interfaces;
using CatalogService.Domain.Entities;
using CatalogService.Domain.Interfaces;

namespace CatalogService.Application.Services;

public class ProductService(IProductRepository productRepository) : IProductService
{
    private readonly IProductRepository _productRepository = productRepository;

    public async Task<ProductDto?> GetByIdAsync(int id)
    {
        Product? product = await _productRepository.GetByIdAsync(id);
        return product == null ? null : MapToDto(product);
    }

    public async Task<IEnumerable<ProductDto>> GetAllAsync(int? categoryId = null, int page = 1, int pageSize = 10)
    {
        IEnumerable<Product> products = await _productRepository.GetAllAsync();

        if (categoryId.HasValue)
        {
            products = products.Where(p => p.CategoryId == categoryId.Value);
        }

        products = products.Skip((page - 1) * pageSize).Take(pageSize);

        return products.Select(MapToDto);
    }

    public async Task<ProductDto> AddAsync(ProductDto productDto)
    {
        Product product = MapToEntity(productDto);
        await _productRepository.AddAsync(product);
        return MapToDto(product);
    }

    public async Task UpdateAsync(ProductDto productDto)
    {
        Product product = MapToEntity(productDto);
        await _productRepository.UpdateAsync(product);
    }

    public async Task DeleteAsync(int id)
    {
        await _productRepository.DeleteAsync(id);
    }

    public async Task DeleteByCategoryIdAsync(int categoryId)
    {
        await _productRepository.DeleteByCategoryIdAsync(categoryId);
    }

    // Mapping helpers
    private ProductDto MapToDto(Product product) =>
        new()
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            ImageUrl = product.ImageUrl,
            CategoryId = product.CategoryId,
            Price = product.Price,
            Amount = product.Amount
        };

    private static Product MapToEntity(ProductDto dto) =>
        new()
        {
            Id = dto.Id,
            Name = dto.Name,
            Description = dto.Description,
            ImageUrl = dto.ImageUrl,
            CategoryId = dto.CategoryId,
            Price = dto.Price,
            Amount = dto.Amount
        };
}
