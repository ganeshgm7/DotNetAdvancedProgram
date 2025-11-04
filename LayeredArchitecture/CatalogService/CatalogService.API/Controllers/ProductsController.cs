using CatalogService.Application.DTOs;
using CatalogService.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CatalogService.API.Controllers;

/// <summary>
/// Controller for managing products in the catalog.
/// Provides endpoints to retrieve, create, update, and delete products.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ProductsController(IProductService productService) : ControllerBase
{
    private readonly IProductService _productService = productService;

    /// <summary>
    /// Retrieves a product by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the product.</param>
    /// <returns>
    /// Returns <see cref="OkObjectResult"/> with the product details and related links if found;
    /// otherwise, returns <see cref="NotFoundResult"/>.
    /// </returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        ProductDto? product = await _productService.GetByIdAsync(id);

        if (product == null)
        {
            return NotFound();
        }
        return Ok(new
        {
            product,
            links = new[]
            {
                new { rel = "self", href = Url.Action(nameof(Get), new { id }) },
                new { rel = "update", href = Url.Action(nameof(Update), new { id }) },
                new { rel = "delete", href = Url.Action(nameof(Delete), new { id }) }
            }
        });
    }

    /// <summary>
    /// Retrieves a paginated list of products, optionally filtered by category.
    /// </summary>
    /// <param name="categoryId">Optional category identifier to filter products.</param>
    /// <param name="page">The page number for pagination (default is 1).</param>
    /// <param name="pageSize">The number of products per page (default is 10).</param>
    /// <returns>
    /// Returns <see cref="OkObjectResult"/> containing a list of products.
    /// </returns>
    [HttpGet]
    public async Task<IActionResult> List([FromQuery] int? categoryId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        IEnumerable<ProductDto> products = await _productService.GetAllAsync(categoryId, page, pageSize);
        return Ok(products);
    }

    /// <summary>
    /// Creates a new product in the catalog.
    /// </summary>
    /// <param name="product">The product data to create.</param>
    /// <returns>
    /// Returns <see cref="CreatedAtActionResult"/> with the created product details.
    /// </returns>
    [HttpPost]
    public async Task<IActionResult> Add([FromBody] ProductDto product)
    {
        ProductDto createdProduct = await _productService.AddAsync(product);
        return CreatedAtAction(nameof(Get), new { id = createdProduct.Id }, createdProduct);
    }

    /// <summary>
    /// Updates an existing product by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the product to update.</param>
    /// <param name="product">The updated product data.</param>
    /// <returns>
    /// Returns <see cref="NoContentResult"/> if the update is successful.
    /// </returns>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] ProductDto product)
    {
        product.Id = id;

        await _productService.UpdateAsync(product);
        return NoContent();
    }

    /// <summary>
    /// Deletes a product by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the product to delete.</param>
    /// <returns>
    /// Returns <see cref="NoContentResult"/> if the deletion is successful.
    /// </returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _productService.DeleteAsync(id);
        return NoContent();
    }
}