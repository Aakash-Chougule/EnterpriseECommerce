namespace EnterpriseECommerce.Application.DTOs;

public class CheckoutPreviewRequest
{
    public string ShippingState { get; set; } =
        string.Empty;

    public string ShippingStateCode { get; set; } =
        string.Empty;

    public string? PostalCode { get; set; }
}