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

    public async Task<IEnumerable<ProductDto>> GetAllAsync()
    {
        IEnumerable<Product> products = await _productRepository.GetAllAsync();
        return products.Select(MapToDto);
    }

    public async Task AddAsync(ProductDto productDto)
    {
        Product product = MapToEntity(productDto);
        await _productRepository.AddAsync(product);
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
