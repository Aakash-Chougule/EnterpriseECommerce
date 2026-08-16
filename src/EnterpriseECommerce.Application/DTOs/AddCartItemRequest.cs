namespace EnterpriseECommerce.Application.DTOs;

/// <summary>
/// Represents the data required to add a product to a cart.
/// </summary>
public class AddCartItemRequest
{
    public Guid ProductId { get; set; }

    public int Quantity { get; set; }
}