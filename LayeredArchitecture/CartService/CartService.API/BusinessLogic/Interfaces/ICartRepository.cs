using CartService.API.BusinessLogic.Models;

namespace CartService.API.BusinessLogic.Interfaces;

public interface ICartRepository
{
    Task<Cart?> GetCartAsync(string cartId);
    Task AddItemAsync(string cartId, CartItem item);
    Task RemoveItemAsync(string cartId, int itemId);
}
