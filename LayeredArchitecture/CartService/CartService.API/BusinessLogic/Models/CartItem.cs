namespace CartService.API.BusinessLogic.Models;

public class CartItem
{
    public int Id { get; set; }               // ProductId
    public string Name { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public string? ImageAlt { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
}
