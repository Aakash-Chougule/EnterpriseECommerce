namespace EnterpriseECommerce.Application.DTOs;

/// <summary>
/// Represents a product inside a shopping cart.
///
/// Product information such as name and current price is included
/// so frontend applications do not need to make a separate API
/// request for every cart item.
/// </summary>
public class CartItemDto
{
    public Guid Id { get; set; }

    public Guid ProductId { get; set; }

    public string ProductName { get; set; } =
        string.Empty;

    /// <summary>
    /// Current price of the product.
    ///
    /// Important:
    /// The cart displays the current product price.
    /// The final purchase price is permanently copied into
    /// OrderItem during checkout.
    /// </summary>
    public decimal UnitPrice { get; set; }

    public int Quantity { get; set; }

    /// <summary>
    /// Price for this cart line.
    ///
    /// Example:
    /// 3499 × 2 = 6998
    /// </summary>
    public decimal TotalPrice { get; set; }
}