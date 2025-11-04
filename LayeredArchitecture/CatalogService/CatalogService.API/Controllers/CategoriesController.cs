using CatalogService.Application.DTOs;
using CatalogService.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CatalogService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController(ICategoryService categoryService, IProductService productService) : ControllerBase
{
    private readonly ICategoryService _categoryService = categoryService;
    private readonly IProductService _productService = productService;

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        CategoryDto? category = await _categoryService.GetByIdAsync(id);
        if (category == null) return NotFound();

        return Ok(new
        {
            category,
            links = new[]
            {
                new { rel = "self", href = Url.Action(nameof(Get), new { id }) },
                new { rel = "update", href = Url.Action(nameof(Update), new { id }) },
                new { rel = "delete", href = Url.Action(nameof(Delete), new { id }) },
                new { rel = "products", href = Url.Action("List", "Product", new { categoryId = id }) }
            }
        });
    }

    [HttpGet]
    public async Task<IActionResult> List()
    {
        IEnumerable<CategoryDto> categories = await _categoryService.GetAllAsync();
        return Ok(categories);
    }

    [HttpPost]
    public async Task<IActionResult> Add([FromBody] CategoryDto category)
    {
        CategoryDto createdCategory = await _categoryService.AddAsync(category);
        return CreatedAtAction(nameof(Get), new { id = createdCategory.Id }, createdCategory);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] CategoryDto category)
    {
        category.Id = id;

        await _categoryService.UpdateAsync(category);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _productService.DeleteByCategoryIdAsync(id);

        await _categoryService.DeleteAsync(id);
        return NoContent();
    }
}