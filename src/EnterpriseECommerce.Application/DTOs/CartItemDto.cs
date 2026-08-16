namespace EnterpriseECommerce.Application.DTOs;

/// <summary>
/// Represents a product inside a shopping cart.
/// </summary>
public class CartItemDto
{
    public Guid Id { get; set; }

    public Guid ProductId { get; set; }

    public int Quantity { get; set; }
}