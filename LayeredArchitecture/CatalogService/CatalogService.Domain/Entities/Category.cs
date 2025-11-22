namespace CatalogService.Domain.Entities;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = null!; // Required, max length 50
    public string? ImageUrl { get; set; }     // Optional
    public int? ParentCategoryId { get; set; } // Optional
    public Category? ParentCategory { get; set; }
    public ICollection<Category>? SubCategories { get; set; }
    public ICollection<Product>? Products { get; set; }
}
