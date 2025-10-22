namespace CartService.API.BusinessLogic.Models;

public class Cart
{
    public string Id { get; set; }
    public List<CartItem> Items { get; set; } = new List<CartItem>();
}
