namespace EnterpriseECommerce.Application.DTOs;

public class CreateOrderRequest
{
    public string ShippingAddress { get; set; } =
        string.Empty;

    public string PaymentMethod { get; set; } =
        string.Empty;
}