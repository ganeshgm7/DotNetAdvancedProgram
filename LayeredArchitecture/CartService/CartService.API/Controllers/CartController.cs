using CartService.API.BusinessLogic;
using CartService.API.BusinessLogic.Models;
using Microsoft.AspNetCore.Mvc;

namespace CartService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CartController(CartManager cartManager) : ControllerBase
{
    private readonly CartManager _cartManager = cartManager;

    [HttpGet("{cartId}")]
    public async Task<IActionResult> GetCart(string cartId)
    {
        Cart? cart = await _cartManager.GetCartAsync(cartId);
        return cart == null ? NotFound() : Ok(cart);
    }

    [HttpPost("{cartId}/items")]
    public async Task<IActionResult> AddItem(string cartId, [FromBody] CartItem item)
    {
        await _cartManager.AddItemAsync(cartId, item);
        return Ok();
    }

    [HttpDelete("{cartId}/items/{itemId}")]
    public async Task<IActionResult> RemoveItem(string cartId, int itemId)
    {
        await _cartManager.RemoveItemAsync(cartId, itemId);
        return Ok();
    }
}
