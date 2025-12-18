using CatalogService.API.Authorization;
using CatalogService.Application.DTOs;
using CatalogService.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CatalogService.API.Controllers;

/// <summary>
/// Controller for managing product categories in the catalog.
/// Provides endpoints to retrieve, create, update, and delete categories.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CategoriesController(ICategoryService categoryService, IProductService productService) : ControllerBase
{
    private readonly ICategoryService _categoryService = categoryService;
    private readonly IProductService _productService = productService;

    /// <summary>
    /// Retrieves a category by its unique identifier.
    /// All authenticated users can access this endpoint.
    /// </summary>
    /// <param name="id">The unique identifier of the category.</param>
    /// <returns>
    /// Returns <see cref="OkObjectResult"/> with the category details and related links if found;
    /// otherwise, returns <see cref="NotFoundResult"/>.
    /// </returns>
    [HttpGet("{id}")]
    // No policy restriction - all authenticated users can read
    public async Task<IActionResult> Get(int id)
    {
        CategoryDto? category = await _categoryService.GetByIdAsync(id);

        if (category == null)
        {
            return NotFound();
        }

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

    /// <summary>
    /// Retrieves all categories in the catalog.
    /// All authenticated users can access this endpoint.
    /// </summary>
    /// <returns>
    /// Returns <see cref="OkObjectResult"/> containing a list of all categories.
    /// </returns>
    [HttpGet]
    // No policy restriction - all authenticated users can read
    public async Task<IActionResult> List()
    {
        IEnumerable<CategoryDto> categories = await _categoryService.GetAllAsync();
        return Ok(categories);
    }

    /// <summary>
    /// Creates a new category in the catalog.
    /// Only accessible by users with Manager role.
    /// </summary>
    /// <param name="category">The category data to create.</param>
    /// <returns>
    /// Returns <see cref="CreatedAtActionResult"/> with the created category details.
    /// </returns>
    [HttpPost]
    [Authorize(Policy = Policies.CanCreate)]
    public async Task<IActionResult> Add([FromBody] CategoryDto category)
    {
        CategoryDto createdCategory = await _categoryService.AddAsync(category);
        return CreatedAtAction(nameof(Get), new { id = createdCategory.Id }, createdCategory);
    }

    /// <summary>
    /// Updates an existing category by its unique identifier.
    /// Only accessible by users with Manager role.
    /// </summary>
    /// <param name="id">The unique identifier of the category to update.</param>
    /// <param name="category">The updated category data.</param>
    /// <returns>
    /// Returns <see cref="NoContentResult"/> if the update is successful.
    /// </returns>
    [HttpPut("{id}")]
    [Authorize(Policy = Policies.CanUpdate)]
    public async Task<IActionResult> Update(int id, [FromBody] CategoryDto category)
    {
        category.Id = id;

        await _categoryService.UpdateAsync(category);
        return NoContent();
    }

    /// <summary>
    /// Deletes a category and its associated products by category identifier.
    /// Only accessible by users with Manager role.
    /// </summary>
    /// <param name="id">The unique identifier of the category to delete.</param>
    /// <returns>
    /// Returns <see cref="NoContentResult"/> if the deletion is successful.
    /// </returns>
    [HttpDelete("{id}")]
    [Authorize(Policy = Policies.CanDelete)]
    public async Task<IActionResult> Delete(int id)
    {
        await _productService.DeleteByCategoryIdAsync(id);

        await _categoryService.DeleteAsync(id);
        return NoContent();
    }
}