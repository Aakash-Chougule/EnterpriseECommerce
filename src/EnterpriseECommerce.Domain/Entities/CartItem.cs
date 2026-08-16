namespace EnterpriseECommerce.Domain.Entities;

/// <summary>
/// Represents a product inside a shopping cart.
/// </summary>
public class CartItem
{
    public Guid Id { get; private set; }

    public Guid CartId { get; private set; }

    public Guid ProductId { get; private set; }

    public int Quantity { get; private set; }

    // ------------------------------------------------------------
    // EF Core constructor
    // ------------------------------------------------------------

    private CartItem()
    {
    }

    // ------------------------------------------------------------
    // Application constructor
    // ------------------------------------------------------------

    public CartItem(
        Guid cartId,
        Guid productId,
        int quantity)
    {
        if (cartId == Guid.Empty)
        {
            throw new ArgumentException(
                "CartId is required.");
        }

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

        Id = Guid.NewGuid();
        CartId = cartId;
        ProductId = productId;
        Quantity = quantity;
    }

    // ------------------------------------------------------------
    // Domain methods
    // ------------------------------------------------------------

    public void IncreaseQuantity(int quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentException(
                "Quantity must be greater than zero.");
        }

        Quantity += quantity;
    }

    public void UpdateQuantity(int quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentException(
                "Quantity must be greater than zero.");
        }

        Quantity = quantity;
    }
}