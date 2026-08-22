namespace EnterpriseECommerce.Application.DTOs;

// ============================================================
// REPORT DASHBOARD
// ============================================================

public class ReportDashboardDto
{
    // ========================================================
    // FINANCIAL
    // ========================================================

    public decimal TotalRevenue { get; set; }

    public decimal AverageOrderValue { get; set; }

    // ========================================================
    // ORDERS
    // ========================================================

    public int TotalOrders { get; set; }

    public int PendingOrders { get; set; }

    public int ConfirmedOrders { get; set; }

    public int ProcessingOrders { get; set; }

    public int ShippedOrders { get; set; }

    public int DeliveredOrders { get; set; }

    public int CancelledOrders { get; set; }

    // ========================================================
    // PAYMENTS
    // ========================================================

    public int TotalPayments { get; set; }

    public int SuccessfulPayments { get; set; }

    public int PendingPayments { get; set; }

    public int FailedPayments { get; set; }

    public int RefundedPayments { get; set; }

    // ========================================================
    // DETAILS
    // ========================================================

    public List<TopProductReportDto>
        TopProducts
    { get; set; } = [];

    public List<PaymentMethodReportDto>
        PaymentMethods
    { get; set; } = [];

    public List<RecentOrderReportDto>
        RecentOrders
    { get; set; } = [];
}

// ============================================================
// TOP PRODUCT
// ============================================================

public class TopProductReportDto
{
    public Guid ProductId { get; set; }

    public string ProductName { get; set; } =
        string.Empty;

    public int QuantitySold { get; set; }

    public decimal Revenue { get; set; }
}

// ============================================================
// PAYMENT METHOD
// ============================================================

public class PaymentMethodReportDto
{
    public string PaymentMethod { get; set; } =
        string.Empty;

    public int Count { get; set; }

    public decimal Amount { get; set; }
}

// ============================================================
// RECENT ORDER
// ============================================================

public class RecentOrderReportDto
{
    public Guid OrderId { get; set; }

    public string OrderNumber { get; set; } =
        string.Empty;

    // ========================================================
    // PRODUCT INFORMATION
    // ========================================================

    public string ProductNames { get; set; } =
        string.Empty;

    public int TotalQuantity { get; set; }

    public List<ReportOrderItemDto>
        Items
    { get; set; } = [];

    // ========================================================
    // ORDER INFORMATION
    // ========================================================

    public decimal TotalAmount { get; set; }

    public string Status { get; set; } =
        string.Empty;

    public string PaymentStatus { get; set; } =
        string.Empty;

    public DateTime CreatedAt { get; set; }
}

// ============================================================
// REPORT ORDER ITEM
// ============================================================

public class ReportOrderItemDto
{
    public Guid ProductId { get; set; }

    public string ProductName { get; set; } =
        string.Empty;

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal TotalPrice { get; set; }
}