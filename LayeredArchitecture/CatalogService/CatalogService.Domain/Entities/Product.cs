namespace CatalogService.Domain.Entities;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = null!; // Required, max length 50
    public string? Description { get; set; }  // Optional, can contain HTML
    public string? ImageUrl { get; set; }     // Optional
    public int CategoryId { get; set; }       // Required
    public Category Category { get; set; } = null!;
    public decimal Price { get; set; }        // Required
    public int Amount { get; set; }           // Required, positive int
}
