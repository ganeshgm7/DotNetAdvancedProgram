using CartService.API.BusinessLogic.Interfaces;
using CartService.API.BusinessLogic.Models;

namespace CartService.API.BusinessLogic;

public class CartManager(ICartRepository cartRepository)
{
    private readonly ICartRepository _cartRepository = cartRepository;

    public Task<Cart?> GetCartAsync(string cartId) => _cartRepository.GetCartAsync(cartId);

    public Task AddItemAsync(string cartId, CartItem item) => _cartRepository.AddItemAsync(cartId, item);

    public Task RemoveItemAsync(string cartId, int itemId) => _cartRepository.RemoveItemAsync(cartId, itemId);
}
