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

    public IReadOnlyCollection<OrderItem> OrderItems => _orderItems.AsReadOnly();

    private Order()
    {
    }

    public Order(Guid userId, string shippingAddress)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User is required.");

        if (string.IsNullOrWhiteSpace(shippingAddress))
            throw new ArgumentException("Shipping address is required.");

        Id = Guid.NewGuid();
        OrderNumber = $"ORD-{DateTime.UtcNow:yyyyMMddHHmmss}-{Random.Shared.Next(1000, 9999)}";
        UserId = userId;
        ShippingAddress = shippingAddress;
        Status = OrderStatus.Pending;
        PaymentStatus = PaymentStatus.Pending;
        CreatedAt = DateTime.UtcNow;
    }

    public void AddItem(OrderItem item)
    {
        ArgumentNullException.ThrowIfNull(item);

        _orderItems.Add(item);

        CalculateTotal();
    }

    public void Confirm()
    {
        Status = OrderStatus.Confirmed;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        if (Status == OrderStatus.Shipped ||
            Status == OrderStatus.Delivered)
        {
            throw new InvalidOperationException(
                "A shipped or delivered order cannot be cancelled.");
        }

        Status = OrderStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkPaymentSuccessful()
    {
        PaymentStatus = PaymentStatus.Success;
        UpdatedAt = DateTime.UtcNow;
    }

    private void CalculateTotal()
    {
        TotalAmount = _orderItems.Sum(x => x.TotalPrice);
    }
}