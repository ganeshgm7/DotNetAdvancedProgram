using CatalogService.Application.DTOs;
using CatalogService.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CatalogService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController(IProductService productService) : ControllerBase
{
    private readonly IProductService _productService = productService;

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        ProductDto? product = await _productService.GetByIdAsync(id);
        if (product == null) return NotFound();

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

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] int? categoryId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        IEnumerable<ProductDto> products = await _productService.GetAllAsync(categoryId, page, pageSize);
        return Ok(products);
    }

    [HttpPost]
    public async Task<IActionResult> Add([FromBody] ProductDto product)
    {
        ProductDto createdProduct = await _productService.AddAsync(product);
        return CreatedAtAction(nameof(Get), new { id = createdProduct.Id }, createdProduct);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] ProductDto product)
    {
        product.Id = id;

        await _productService.UpdateAsync(product);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _productService.DeleteAsync(id);
        return NoContent();
    }
}