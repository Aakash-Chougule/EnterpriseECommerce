namespace EnterpriseECommerce.Domain.Entities;

/// <summary>
/// Represents a shopping cart belonging to a user.
///
/// A cart contains the products the user intends to purchase.
/// </summary>
public class Cart
{
    public Guid Id { get; private set; }

    /// <summary>
    /// User who owns this cart.
    /// </summary>
    public Guid UserId { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime? UpdatedAt { get; private set; }

    /// <summary>
    /// Products currently present in the cart.
    /// </summary>
    public ICollection<CartItem> Items { get; private set; }
        = new List<CartItem>();

    // ------------------------------------------------------------
    // EF Core constructor
    // ------------------------------------------------------------

    private Cart()
    {
    }

    // ------------------------------------------------------------
    // Application constructor
    // ------------------------------------------------------------

    public Cart(Guid userId)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException(
                "UserId is required.");
        }

        Id = Guid.NewGuid();
        UserId = userId;
        CreatedAt = DateTime.UtcNow;
    }

    // ------------------------------------------------------------
    // Domain methods
    // ------------------------------------------------------------

    public void AddItem(
        Guid productId,
        int quantity)
    {
        if (productId == Guid.Empty)
        {
            throw new ArgumentException(
                "ProductId is required.");
        }

        if (quantity <= 0)
        {
            throw new ArgumentException(
                "Quantity must be greater than zero.");
        }

        var existingItem = Items.FirstOrDefault(
            item => item.ProductId == productId);

        if (existingItem is not null)
        {
            existingItem.IncreaseQuantity(quantity);
        }
        else
        {
            Items.Add(new CartItem(
                Id,
                productId,
                quantity));
        }

        UpdatedAt = DateTime.UtcNow;
    }

    public void RemoveItem(Guid productId)
    {
        var item = Items.FirstOrDefault(
            item => item.ProductId == productId);

        if (item is null)
        {
            return;
        }

        Items.Remove(item);

        UpdatedAt = DateTime.UtcNow;
    }

    public void Clear()
    {
        Items.Clear();

        UpdatedAt = DateTime.UtcNow;
    }
}