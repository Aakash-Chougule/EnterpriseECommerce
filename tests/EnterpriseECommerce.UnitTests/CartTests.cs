using EnterpriseECommerce.Domain.Entities;

namespace EnterpriseECommerce.UnitTests;

public class CartTests
{
    // ============================================================
    // HELPERS
    // ============================================================

    private static Cart CreateCart()
    {
        return new Cart(Guid.NewGuid());
    }

    // ============================================================
    // CONSTRUCTOR
    // ============================================================

    [Fact]
    public void Constructor_WithValidUserId_CreatesCart()
    {
        var userId = Guid.NewGuid();

        var cart = new Cart(userId);

        Assert.NotEqual(Guid.Empty, cart.Id);
        Assert.Equal(userId, cart.UserId);
        Assert.Empty(cart.Items);
    }

    [Fact]
    public void Constructor_WithEmptyUserId_ThrowsException()
    {
        var exception =
            Assert.Throws<ArgumentException>(
                () => new Cart(Guid.Empty));

        Assert.Equal(
            "UserId is required.",
            exception.Message);
    }

    // ============================================================
    // ADD ITEM
    // ============================================================

    [Fact]
    public void AddItem_NewProduct_AddsItem()
    {
        var cart = CreateCart();

        var productId =
            Guid.NewGuid();

        cart.AddItem(
            productId,
            2);

        Assert.Single(cart.Items);

        var item =
            cart.Items.First();

        Assert.Equal(
            productId,
            item.ProductId);

        Assert.Equal(
            2,
            item.Quantity);
    }

    [Fact]
    public void AddItem_ExistingProduct_IncreasesQuantity()
    {
        var cart = CreateCart();

        var productId =
            Guid.NewGuid();

        cart.AddItem(
            productId,
            2);

        cart.AddItem(
            productId,
            3);

        Assert.Single(cart.Items);

        Assert.Equal(
            5,
            cart.Items.First().Quantity);
    }

    [Fact]
    public void AddItem_WithEmptyProductId_ThrowsException()
    {
        var cart = CreateCart();

        var exception =
            Assert.Throws<ArgumentException>(
                () => cart.AddItem(
                    Guid.Empty,
                    1));

        Assert.Equal(
            "ProductId is required.",
            exception.Message);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void AddItem_WithInvalidQuantity_ThrowsException(
        int quantity)
    {
        var cart = CreateCart();

        var exception =
            Assert.Throws<ArgumentException>(
                () => cart.AddItem(
                    Guid.NewGuid(),
                    quantity));

        Assert.Equal(
            "Quantity must be greater than zero.",
            exception.Message);
    }

    // ============================================================
    // REMOVE ITEM
    // ============================================================

    [Fact]
    public void RemoveItem_RemovesExistingItem()
    {
        var cart = CreateCart();

        var productId =
            Guid.NewGuid();

        cart.AddItem(
            productId,
            2);

        cart.RemoveItem(
            productId);

        Assert.Empty(
            cart.Items);
    }

    [Fact]
    public void RemoveItem_ItemDoesNotExist_DoesNothing()
    {
        var cart = CreateCart();

        cart.RemoveItem(
            Guid.NewGuid());

        Assert.Empty(
            cart.Items);
    }

    // ============================================================
    // UPDATE QUANTITY
    // ============================================================

    [Fact]
    public void UpdateItemQuantity_UpdatesQuantity()
    {
        var cart = CreateCart();

        var productId =
            Guid.NewGuid();

        cart.AddItem(
            productId,
            1);

        cart.UpdateItemQuantity(
            productId,
            10);

        Assert.Equal(
            10,
            cart.Items.First().Quantity);
    }

    [Fact]
    public void UpdateItemQuantity_WithEmptyProductId_ThrowsException()
    {
        var cart = CreateCart();

        var exception =
            Assert.Throws<ArgumentException>(
                () =>
                    cart.UpdateItemQuantity(
                        Guid.Empty,
                        5));

        Assert.Equal(
            "ProductId is required.",
            exception.Message);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void UpdateItemQuantity_WithInvalidQuantity_ThrowsException(
        int quantity)
    {
        var cart = CreateCart();

        var exception =
            Assert.Throws<ArgumentException>(
                () =>
                    cart.UpdateItemQuantity(
                        Guid.NewGuid(),
                        quantity));

        Assert.Equal(
            "Quantity must be greater than zero.",
            exception.Message);
    }

    [Fact]
    public void UpdateItemQuantity_WhenItemNotFound_ThrowsException()
    {
        var cart = CreateCart();

        var exception =
            Assert.Throws<KeyNotFoundException>(
                () =>
                    cart.UpdateItemQuantity(
                        Guid.NewGuid(),
                        5));

        Assert.Equal(
            "Cart item not found.",
            exception.Message);
    }

    // ============================================================
    // CLEAR
    // ============================================================

    [Fact]
    public void Clear_RemovesAllItems()
    {
        var cart = CreateCart();

        cart.AddItem(
            Guid.NewGuid(),
            2);

        cart.AddItem(
            Guid.NewGuid(),
            3);

        Assert.Equal(
            2,
            cart.Items.Count);

        cart.Clear();

        Assert.Empty(
            cart.Items);
    }

    // ============================================================
    // UPDATED AT
    // ============================================================

    [Fact]
    public void AddItem_SetsUpdatedAt()
    {
        var cart = CreateCart();

        Assert.Null(
            cart.UpdatedAt);

        cart.AddItem(
            Guid.NewGuid(),
            1);

        Assert.NotNull(
            cart.UpdatedAt);
    }

    [Fact]
    public void RemoveItem_SetsUpdatedAt()
    {
        var cart = CreateCart();

        var productId =
            Guid.NewGuid();

        cart.AddItem(
            productId,
            1);

        cart.RemoveItem(
            productId);

        Assert.NotNull(
            cart.UpdatedAt);
    }

    [Fact]
    public void UpdateQuantity_SetsUpdatedAt()
    {
        var cart = CreateCart();

        var productId =
            Guid.NewGuid();

        cart.AddItem(
            productId,
            1);

        cart.UpdateItemQuantity(
            productId,
            5);

        Assert.NotNull(
            cart.UpdatedAt);
    }

    [Fact]
    public void Clear_SetsUpdatedAt()
    {
        var cart = CreateCart();

        cart.AddItem(
            Guid.NewGuid(),
            1);

        cart.Clear();

        Assert.NotNull(
            cart.UpdatedAt);
    }
}