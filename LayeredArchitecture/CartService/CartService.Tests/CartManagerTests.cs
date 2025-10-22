using CartService.API.BusinessLogic;
using CartService.API.BusinessLogic.Interfaces;
using CartService.API.BusinessLogic.Models;
using Moq;
using Xunit;

namespace CartService.Tests;

public class CartManagerTests
{
    [Fact]
    public async Task AddItemAsync_CallsRepositoryWithCorrectParameters()
    {
        // Arrange
        Mock<ICartRepository> mockRepo = new();
        CartManager cartManager = new(mockRepo.Object);
        CartItem item = new()
        {
            Id = 1,
            Name = "Test Item",
            Price = 10.0m,
            Quantity = 2
        };

        string cartId = "cart123";

        // Act
        await cartManager.AddItemAsync(cartId, item);

        // Assert
        mockRepo.Verify(r => r.AddItemAsync(cartId, item), Times.Once);
    }

    [Fact]
    public async Task GetCartAsync_ReturnsCartFromRepository()
    {
        // Arrange
        Mock<ICartRepository> mockRepo = new();
        Cart expectedCart = new()
        {
            Id = "cart123"
        };

        mockRepo.Setup(r => r.GetCartAsync("cart123")).ReturnsAsync(expectedCart);
        CartManager cartManager = new(mockRepo.Object);

        // Act
        Cart? result = await cartManager.GetCartAsync("cart123");

        // Assert
        Assert.Equal(expectedCart, result);
    }
}
