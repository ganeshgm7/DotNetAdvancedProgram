using CartService.API.BusinessLogic.Interfaces;
using CartService.API.BusinessLogic.Models;
using LiteDB;

namespace CartService.API.DataAccess;

public class CartRepository(string dbPath) : ICartRepository
{
    private readonly string _dbPath = dbPath;

    public Task<Cart?> GetCartAsync(string cartId)
    {
        using LiteDatabase db = new(_dbPath);

        ILiteCollection<Cart> carts = db.GetCollection<Cart>("carts");
        Cart cart = carts.FindById(cartId);

        return Task.FromResult(cart);
    }

    public Task AddItemAsync(string cartId, CartItem item)
    {
        using LiteDatabase db = new(_dbPath);

        ILiteCollection<Cart> carts = db.GetCollection<Cart>("carts");
        Cart cart = carts.FindById(cartId) ?? new Cart { Id = cartId };

        CartItem? existing = cart.Items.FirstOrDefault(i => i.Id == item.Id);

        if (existing != null)
        {
            existing.Quantity += item.Quantity;
        }
        else
        {
            cart.Items.Add(item);
        }

        carts.Upsert(cart);
        return Task.CompletedTask;
    }

    public Task RemoveItemAsync(string cartId, int itemId)
    {
        using LiteDatabase db = new(_dbPath);

        ILiteCollection<Cart> carts = db.GetCollection<Cart>("carts");
        Cart cart = carts.FindById(cartId);

        if (cart != null)
        {
            cart.Items.RemoveAll(i => i.Id == itemId);
            carts.Update(cart);
        }

        return Task.CompletedTask;
    }
}
