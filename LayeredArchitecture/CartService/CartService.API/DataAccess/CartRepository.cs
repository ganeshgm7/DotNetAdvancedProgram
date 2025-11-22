using CartService.API.BusinessLogic.Interfaces;
using CartService.API.BusinessLogic.Models;
using LiteDB;
using System.Linq;

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

    public Task UpdateProductMetadataAsync(int productId, string name, decimal price)
    {
        using LiteDatabase db = new(_dbPath);
        ILiteCollection<Cart> carts = db.GetCollection<Cart>("carts");

        foreach (Cart cart in carts.FindAll())
        {
            bool changed = false;

            foreach (CartItem item in cart.Items.Where(i => i.Id == productId))
            {
                item.Name = name;
                item.Price = price;
                changed = true;
            }
            if (changed)
            {
                carts.Update(cart);
            }
        }
        return Task.CompletedTask;
    }

    public Task RemoveProductFromAllCartsAsync(int productId)
    {
        using LiteDatabase db = new(_dbPath);
        ILiteCollection<Cart> carts = db.GetCollection<Cart>("carts");

        foreach (Cart cart in carts.FindAll())
        {
            int removed = cart.Items.RemoveAll(i => i.Id == productId);

            if (removed > 0)
            {
                carts.Update(cart);
            }
        }
        return Task.CompletedTask;
    }
}
