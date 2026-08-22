using EnterpriseECommerce.Domain.Enums;

namespace EnterpriseECommerce.Application.DTOs;

public class OrderDto
{
    public Guid Id { get; set; }

    public string OrderNumber { get; set; } =
        string.Empty;

    public Guid UserId { get; set; }

    // ========================================================
    // CUSTOMER
    // ========================================================

    public string CustomerName { get; set; } =
        string.Empty;

    public string CustomerEmail { get; set; } =
        string.Empty;

    public string? CustomerPhoneNumber { get; set; }

    // ========================================================
    // FINANCIAL
    // ========================================================

    public decimal Subtotal { get; set; }

    public decimal TaxableAmount { get; set; }

    public decimal TotalGst { get; set; }

    public decimal TotalCgst { get; set; }

    public decimal TotalSgst { get; set; }

    public decimal TotalIgst { get; set; }

    public decimal ShippingCharge { get; set; }

    public decimal DiscountAmount { get; set; }

    public decimal TotalAmount { get; set; }

    // ========================================================
    // STATUS
    // ========================================================

    public OrderStatus Status { get; set; }

    public PaymentStatus PaymentStatus { get; set; }

    // ========================================================
    // SHIPPING
    // ========================================================

    public string ShippingAddress { get; set; } =
        string.Empty;

    public string ShippingState { get; set; } =
        string.Empty;

    public string ShippingStateCode { get; set; } =
        string.Empty;

    public string PostalCode { get; set; } =
        string.Empty;

    public bool IsInterState { get; set; }

    // ========================================================
    // DATES
    // ========================================================

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public List<OrderItemDto>
        OrderItems
    { get; set; } =
            new();
}