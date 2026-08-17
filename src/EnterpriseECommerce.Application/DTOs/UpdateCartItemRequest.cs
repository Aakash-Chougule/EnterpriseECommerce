namespace EnterpriseECommerce.Application.DTOs;

/// <summary>
/// Request used to change the quantity of an existing cart item.
/// </summary>
public class UpdateCartItemRequest
{
    public int Quantity { get; set; }
}