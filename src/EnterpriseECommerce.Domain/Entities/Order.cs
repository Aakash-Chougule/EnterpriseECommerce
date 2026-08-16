using EnterpriseECommerce.Domain.Enums;

namespace EnterpriseECommerce.Domain.Entities;

public class Order
{
    private readonly List<OrderItem> _orderItems = new();

    public Guid Id { get; private set; }

    public string OrderNumber { get; private set; } = string.Empty;

    public Guid UserId { get; private set; }

    public decimal TotalAmount { get; private set; }

    public OrderStatus Status { get; private set; }

    public PaymentStatus PaymentStatus { get; private set; }

    public string ShippingAddress { get; private set; } = string.Empty;

    public DateTime CreatedAt { get; private set; }

    public DateTime? UpdatedAt { get; private set; }

    public IReadOnlyCollection<OrderItem> OrderItems =>
        _orderItems.AsReadOnly();

    private Order()
    {
    }

    public Order(
        Guid userId,
        string shippingAddress)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException(
                "User is required.");
        }

        if (string.IsNullOrWhiteSpace(shippingAddress))
        {
            throw new ArgumentException(
                "Shipping address is required.");
        }

        Id = Guid.NewGuid();

        OrderNumber =
            $"ORD-{DateTime.UtcNow:yyyyMMddHHmmss}-" +
            $"{Random.Shared.Next(1000, 9999)}";

        UserId = userId;

        ShippingAddress =
            shippingAddress.Trim();

        Status =
            OrderStatus.Pending;

        PaymentStatus =
            PaymentStatus.Pending;

        CreatedAt =
            DateTime.UtcNow;
    }

    // ============================================================
    // ORDER ITEMS
    // ============================================================

    public void AddItem(OrderItem item)
    {
        ArgumentNullException.ThrowIfNull(item);

        _orderItems.Add(item);

        CalculateTotal();
    }

    // ============================================================
    // ORDER STATUS
    // ============================================================

    public void Confirm()
    {
        if (Status != OrderStatus.Pending)
        {
            throw new InvalidOperationException(
                "Only pending orders can be confirmed.");
        }

        Status = OrderStatus.Confirmed;

        UpdatedAt = DateTime.UtcNow;
    }

    public void StartProcessing()
    {
        if (Status != OrderStatus.Confirmed)
        {
            throw new InvalidOperationException(
                "Only confirmed orders can be moved to processing.");
        }

        Status = OrderStatus.Processing;

        UpdatedAt = DateTime.UtcNow;
    }

    public void Ship()
    {
        if (Status != OrderStatus.Processing)
        {
            throw new InvalidOperationException(
                "Only processing orders can be shipped.");
        }

        Status = OrderStatus.Shipped;

        UpdatedAt = DateTime.UtcNow;
    }

    public void Deliver()
    {
        if (Status != OrderStatus.Shipped)
        {
            throw new InvalidOperationException(
                "Only shipped orders can be delivered.");
        }

        Status = OrderStatus.Delivered;

        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        if (Status == OrderStatus.Cancelled)
        {
            throw new InvalidOperationException(
                "Order is already cancelled.");
        }

        if (Status == OrderStatus.Shipped ||
            Status == OrderStatus.Delivered)
        {
            throw new InvalidOperationException(
                "A shipped or delivered order cannot be cancelled.");
        }

        Status = OrderStatus.Cancelled;

        UpdatedAt = DateTime.UtcNow;
    }

    // ============================================================
    // PAYMENT STATUS
    // ============================================================

    public void MarkPaymentSuccessful()
    {
        if (Status == OrderStatus.Cancelled)
        {
            throw new InvalidOperationException(
                "Payment cannot be completed for a cancelled order.");
        }

        if (PaymentStatus == PaymentStatus.Success)
        {
            throw new InvalidOperationException(
                "Payment is already marked as successful.");
        }

        PaymentStatus = PaymentStatus.Success;

        UpdatedAt = DateTime.UtcNow;
    }


    public void MarkPaymentFailed()
    {
        if (PaymentStatus == PaymentStatus.Success)
        {
            throw new InvalidOperationException(
                "A successful payment cannot be marked as failed.");
        }

        PaymentStatus = PaymentStatus.Failed;

        UpdatedAt = DateTime.UtcNow;
    }

    // ============================================================
    // TOTAL
    // ============================================================

    private void CalculateTotal()
    {
        TotalAmount =
            _orderItems.Sum(item => item.TotalPrice);
    }
}