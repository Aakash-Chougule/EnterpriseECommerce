namespace EnterpriseECommerce.Application.DTOs;

/// <summary>
/// Represents a shopping cart returned by the API.
/// </summary>
public class CartDto
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public List<CartItemDto> Items { get; set; } =
        new();

    /// <summary>
    /// Current total value of all products in the cart.
    ///
    /// This value is calculated using the current product prices.
    /// </summary>
    public decimal TotalAmount { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}