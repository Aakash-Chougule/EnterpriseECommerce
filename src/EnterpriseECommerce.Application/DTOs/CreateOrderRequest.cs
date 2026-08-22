namespace EnterpriseECommerce.Application.DTOs;

public class CreateOrderRequest
{
    public string ShippingAddress { get; set; } =
        string.Empty;

    public string ShippingState { get; set; } =
        string.Empty;

    public string ShippingStateCode { get; set; } =
        string.Empty;

    public string PostalCode { get; set; } =
        string.Empty;

    public string PaymentMethod { get; set; } =
        string.Empty;
}