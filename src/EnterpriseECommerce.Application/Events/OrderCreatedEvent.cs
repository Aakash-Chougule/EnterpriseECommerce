namespace EnterpriseECommerce.Application.Events;

public class OrderCreatedEvent
{
    public Guid OrderId { get; set; }

    public string OrderNumber { get; set; } =
        string.Empty;

    public Guid UserId { get; set; }

    public string CustomerEmail { get; set; } =
        string.Empty;

    public string CustomerName { get; set; } =
        string.Empty;

    public decimal TotalAmount { get; set; }

    public string PaymentMethod { get; set; } =
        string.Empty;

    public DateTime CreatedAt { get; set; }
}