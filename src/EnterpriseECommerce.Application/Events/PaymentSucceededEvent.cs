namespace EnterpriseECommerce.Application.Events;

/// <summary>
/// Kafka event published after a payment is completed
/// successfully.
///
/// NotificationService consumes this event and sends
/// a payment confirmation email to the customer.
/// </summary>
public class PaymentSucceededEvent
{
    // ========================================================
    // PAYMENT
    // ========================================================

    public Guid PaymentId { get; set; }

    public Guid OrderId { get; set; }

    public string OrderNumber { get; set; } =
        string.Empty;

    // ========================================================
    // CUSTOMER
    // ========================================================

    public Guid UserId { get; set; }

    public string CustomerEmail { get; set; } =
        string.Empty;

    public string CustomerName { get; set; } =
        string.Empty;

    // ========================================================
    // PAYMENT INFORMATION
    // ========================================================

    public decimal Amount { get; set; }

    public string PaymentMethod { get; set; } =
        string.Empty;

    public string TransactionId { get; set; } =
        string.Empty;

    // ========================================================
    // EVENT TIME
    // ========================================================

    public DateTime PaidAt { get; set; }
}