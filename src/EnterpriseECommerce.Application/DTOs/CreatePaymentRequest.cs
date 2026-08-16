namespace EnterpriseECommerce.Application.DTOs;

public class CreatePaymentRequest
{
    public Guid OrderId { get; set; }

    public string PaymentMethod { get; set; } = string.Empty;
}