using EnterpriseECommerce.Domain.Enums;

namespace EnterpriseECommerce.Application.DTOs;

public class OrderDto
{
    public Guid Id { get; set; }

    public string OrderNumber { get; set; } =
        string.Empty;

    public Guid UserId { get; set; }

    // ========================================================
    // CUSTOMER DETAILS
    // ========================================================

    public string CustomerName { get; set; } =
        string.Empty;

    public string CustomerEmail { get; set; } =
        string.Empty;

    public string? CustomerPhoneNumber { get; set; }

    // ========================================================
    // ORDER DETAILS
    // ========================================================

    public decimal TotalAmount { get; set; }

    public OrderStatus Status { get; set; }

    public PaymentStatus PaymentStatus { get; set; }

    public string ShippingAddress { get; set; } =
        string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public List<OrderItemDto> OrderItems { get; set; }
        = new();
}