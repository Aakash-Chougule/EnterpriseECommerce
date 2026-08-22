using EnterpriseECommerce.Domain.Enums;

namespace EnterpriseECommerce.Domain.Entities;

public class Order
{
    private readonly List<OrderItem>
        _orderItems = new();

    public Guid Id { get; private set; }

    public string OrderNumber { get; private set; } =
        string.Empty;

    public Guid UserId { get; private set; }

    // ========================================================
    // MONEY
    // ========================================================

    /// <summary>
    /// GST-inclusive total of all order items before
    /// shipping and discount.
    /// </summary>
    public decimal Subtotal { get; private set; }

    public decimal TaxableAmount { get; private set; }

    public decimal TotalGst { get; private set; }

    public decimal TotalCgst { get; private set; }

    public decimal TotalSgst { get; private set; }

    public decimal TotalIgst { get; private set; }

    public decimal ShippingCharge { get; private set; }

    public decimal DiscountAmount { get; private set; }

    /// <summary>
    /// Final amount payable by customer.
    /// </summary>
    public decimal TotalAmount { get; private set; }

    // ========================================================
    // STATUS
    // ========================================================

    public OrderStatus Status { get; private set; }

    public PaymentStatus PaymentStatus { get; private set; }

    // ========================================================
    // SHIPPING
    // ========================================================

    public string ShippingAddress { get; private set; } =
        string.Empty;

    public string ShippingState { get; private set; } =
        string.Empty;

    /// <summary>
    /// Indian GST State Code.
    ///
    /// Example:
    /// Maharashtra = 27
    /// Karnataka   = 29
    /// Gujarat     = 24
    /// </summary>
    public string ShippingStateCode { get; private set; } =
        string.Empty;

    public string PostalCode { get; private set; } =
        string.Empty;

    public bool IsInterState { get; private set; }

    // ========================================================
    // DATES
    // ========================================================

    public DateTime CreatedAt { get; private set; }

    public DateTime? UpdatedAt { get; private set; }

    public IReadOnlyCollection<OrderItem>
        OrderItems =>
            _orderItems.AsReadOnly();

    private Order()
    {
    }

    public Order(
        Guid userId,
        string shippingAddress,
        string shippingState,
        string shippingStateCode,
        string postalCode,
        string sellerStateCode)
    {
        if (userId ==
            Guid.Empty)
        {
            throw new ArgumentException(
                "User is required.");
        }

        if (string.IsNullOrWhiteSpace(
            shippingAddress))
        {
            throw new ArgumentException(
                "Shipping address is required.");
        }

        if (string.IsNullOrWhiteSpace(
            shippingState))
        {
            throw new ArgumentException(
                "Shipping state is required.");
        }

        if (string.IsNullOrWhiteSpace(
            shippingStateCode))
        {
            throw new ArgumentException(
                "Shipping state code is required.");
        }

        if (string.IsNullOrWhiteSpace(
            postalCode))
        {
            throw new ArgumentException(
                "PIN code is required.");
        }

        if (string.IsNullOrWhiteSpace(
            sellerStateCode))
        {
            throw new ArgumentException(
                "Seller state code is required.");
        }

        Id =
            Guid.NewGuid();

        OrderNumber =
            $"ORD-{DateTime.UtcNow:yyyyMMddHHmmss}-" +
            $"{Random.Shared.Next(1000, 9999)}";

        UserId =
            userId;

        ShippingAddress =
            shippingAddress.Trim();

        ShippingState =
            shippingState.Trim();

        ShippingStateCode =
            shippingStateCode.Trim();

        PostalCode =
            postalCode.Trim();

        IsInterState =
            !string.Equals(
                sellerStateCode.Trim(),
                shippingStateCode.Trim(),
                StringComparison.OrdinalIgnoreCase);

        Status =
            OrderStatus.Pending;

        PaymentStatus =
            PaymentStatus.Pending;

        CreatedAt =
            DateTime.UtcNow;
    }

    // ========================================================
    // ADD ITEM
    // ========================================================

    public void AddItem(
        OrderItem item)
    {
        ArgumentNullException.ThrowIfNull(
            item);

        _orderItems.Add(
            item);

        CalculateTotals();
    }

    // ========================================================
    // SHIPPING CHARGE
    // ========================================================

    public void SetShippingCharge(
        decimal shippingCharge)
    {
        if (shippingCharge < 0)
        {
            throw new ArgumentException(
                "Shipping charge cannot be negative.");
        }

        ShippingCharge =
            Math.Round(
                shippingCharge,
                2,
                MidpointRounding.AwayFromZero);

        CalculateTotals();
    }

    // ========================================================
    // DISCOUNT
    // ========================================================

    public void SetDiscount(
        decimal discountAmount)
    {
        if (discountAmount < 0)
        {
            throw new ArgumentException(
                "Discount cannot be negative.");
        }

        if (discountAmount >
            Subtotal +
            ShippingCharge)
        {
            throw new InvalidOperationException(
                "Discount cannot exceed order value.");
        }

        DiscountAmount =
            Math.Round(
                discountAmount,
                2,
                MidpointRounding.AwayFromZero);

        CalculateTotals();
    }

    // ========================================================
    // ORDER STATUS
    // ========================================================

    public void Confirm()
    {
        if (Status !=
            OrderStatus.Pending)
        {
            throw new InvalidOperationException(
                "Only pending orders can be confirmed.");
        }

        Status =
            OrderStatus.Confirmed;

        UpdatedAt =
            DateTime.UtcNow;
    }

    public void StartProcessing()
    {
        if (Status !=
            OrderStatus.Confirmed)
        {
            throw new InvalidOperationException(
                "Only confirmed orders can be moved to processing.");
        }

        Status =
            OrderStatus.Processing;

        UpdatedAt =
            DateTime.UtcNow;
    }

    public void Ship()
    {
        if (Status !=
            OrderStatus.Processing)
        {
            throw new InvalidOperationException(
                "Only processing orders can be shipped.");
        }

        Status =
            OrderStatus.Shipped;

        UpdatedAt =
            DateTime.UtcNow;
    }

    public void Deliver()
    {
        if (Status !=
            OrderStatus.Shipped)
        {
            throw new InvalidOperationException(
                "Only shipped orders can be delivered.");
        }

        Status =
            OrderStatus.Delivered;

        UpdatedAt =
            DateTime.UtcNow;
    }

    public void Cancel()
    {
        if (Status ==
            OrderStatus.Cancelled)
        {
            throw new InvalidOperationException(
                "Order is already cancelled.");
        }

        if (Status ==
                OrderStatus.Shipped ||
            Status ==
                OrderStatus.Delivered)
        {
            throw new InvalidOperationException(
                "A shipped or delivered order cannot be cancelled.");
        }

        Status =
            OrderStatus.Cancelled;

        UpdatedAt =
            DateTime.UtcNow;
    }

    // ========================================================
    // PAYMENT
    // ========================================================

    public void MarkPaymentSuccessful()
    {
        if (Status ==
            OrderStatus.Cancelled)
        {
            throw new InvalidOperationException(
                "Payment cannot be completed for a cancelled order.");
        }

        if (PaymentStatus ==
            PaymentStatus.Success)
        {
            throw new InvalidOperationException(
                "Payment is already marked as successful.");
        }

        PaymentStatus =
            PaymentStatus.Success;

        UpdatedAt =
            DateTime.UtcNow;
    }

    public void MarkPaymentFailed()
    {
        if (PaymentStatus ==
            PaymentStatus.Success)
        {
            throw new InvalidOperationException(
                "A successful payment cannot be marked as failed.");
        }

        PaymentStatus =
            PaymentStatus.Failed;

        UpdatedAt =
            DateTime.UtcNow;
    }

    // ========================================================
    // CALCULATE TOTALS
    // ========================================================

    private void CalculateTotals()
    {
        Subtotal =
            Math.Round(
                _orderItems.Sum(
                    item =>
                        item.TotalPrice),
                2,
                MidpointRounding.AwayFromZero);

        TaxableAmount =
            Math.Round(
                _orderItems.Sum(
                    item =>
                        item.TaxableAmount),
                2,
                MidpointRounding.AwayFromZero);

        TotalGst =
            Math.Round(
                _orderItems.Sum(
                    item =>
                        item.GstAmount),
                2,
                MidpointRounding.AwayFromZero);

        TotalCgst =
            Math.Round(
                _orderItems.Sum(
                    item =>
                        item.CgstAmount),
                2,
                MidpointRounding.AwayFromZero);

        TotalSgst =
            Math.Round(
                _orderItems.Sum(
                    item =>
                        item.SgstAmount),
                2,
                MidpointRounding.AwayFromZero);

        TotalIgst =
            Math.Round(
                _orderItems.Sum(
                    item =>
                        item.IgstAmount),
                2,
                MidpointRounding.AwayFromZero);

        TotalAmount =
            Math.Round(
                Subtotal +
                ShippingCharge -
                DiscountAmount,
                2,
                MidpointRounding.AwayFromZero);
    }
}