using Asp.Versioning;
using CartService.API.BusinessLogic;
using CartService.API.BusinessLogic.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CartService.API.Controllers;

/// <summary>
/// Controller for managing shopping cart operations such as retrieving the cart,
/// adding items, and removing items.
/// Accessible to both Manager and StoreCustomer roles.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/cart")]
[Authorize] // All endpoints require authentication
public class CartController(CartManager cartManager) : ControllerBase
{
    private readonly CartManager _cartManager = cartManager;

    /// <summary>
    /// Retrieves the cart for the specified cart ID.
    /// Accessible to all authenticated users (Manager and StoreCustomer).
    /// </summary>
    /// <param name="cartId">The unique identifier of the cart.</param>
    /// <returns>
    /// Returns <see cref="OkObjectResult"/> with the cart if found; otherwise, <see cref="NotFoundResult"/>.
    /// </returns>
    [HttpGet("{cartId}")]
    public async Task<IActionResult> GetCart(string cartId)
    {
        Cart? cart = await _cartManager.GetCartAsync(cartId);
        return cart == null ? NotFound() : Ok(cart);
    }

    /// <summary>
    /// Adds an item to the specified cart.
    /// Accessible to all authenticated users (Manager and StoreCustomer).
    /// </summary>
    /// <param name="cartId">The unique identifier of the cart.</param>
    /// <param name="item">The item to add to the cart.</param>
    /// <returns>
    /// Returns <see cref="OkResult"/> after the item is added.
    /// </returns>
    [HttpPost("{cartId}/items")]
    public async Task<IActionResult> AddItem(string cartId, [FromBody] CartItem item)
    {
        await _cartManager.AddItemAsync(cartId, item);
        return Ok();
    }

    /// <summary>
    /// Removes an item from the specified cart.
    /// Accessible to all authenticated users (Manager and StoreCustomer).
    /// </summary>
    /// <param name="cartId">The unique identifier of the cart.</param>
    /// <param name="itemId">The unique identifier of the item to remove.</param>
    /// <returns>
    /// Returns <see cref="OkResult"/> after the item is removed.
    /// </returns>
    [HttpDelete("{cartId}/items/{itemId}")]
    public async Task<IActionResult> RemoveItem(string cartId, int itemId)
    {
        await _cartManager.RemoveItemAsync(cartId, itemId);
        return Ok();
    }
}