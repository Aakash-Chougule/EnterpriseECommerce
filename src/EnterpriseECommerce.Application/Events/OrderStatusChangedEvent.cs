namespace EnterpriseECommerce.Application.Events;

/// <summary>
/// Event published whenever an order moves to a new status.
///
/// Examples:
/// Pending -> Confirmed
/// Confirmed -> Processing
/// Processing -> Shipped
/// Shipped -> Delivered
/// Any allowed status -> Cancelled
///
/// NotificationService consumes this event and sends
/// a status-update email to the customer.
/// </summary>
public class OrderStatusChangedEvent
{
    // ========================================================
    // ORDER
    // ========================================================

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
    // STATUS
    // ========================================================

    public string PreviousStatus { get; set; } =
        string.Empty;

    public string NewStatus { get; set; } =
        string.Empty;

    // ========================================================
    // ORDER INFORMATION
    // ========================================================

    public decimal TotalAmount { get; set; }

    public string ShippingAddress { get; set; } =
        string.Empty;

    // ========================================================
    // EVENT TIME
    // ========================================================

    public DateTime ChangedAt { get; set; }
}