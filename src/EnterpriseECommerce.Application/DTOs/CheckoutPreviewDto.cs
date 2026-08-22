namespace EnterpriseECommerce.Application.DTOs;

public class CheckoutPreviewDto
{
    // ========================================================
    // ITEMS
    // ========================================================

    public int ProductCount { get; set; }

    public int TotalQuantity { get; set; }

    public List<CheckoutPreviewItemDto>
        Items
    { get; set; } = [];

    // ========================================================
    // TAX
    // ========================================================

    /// <summary>
    /// GST-inclusive total of all products.
    /// </summary>
    public decimal Subtotal { get; set; }

    /// <summary>
    /// Product value before GST.
    /// </summary>
    public decimal TaxableAmount { get; set; }

    /// <summary>
    /// GST already included inside Subtotal.
    /// It is NOT added again.
    /// </summary>
    public decimal TotalGst { get; set; }

    public decimal TotalCgst { get; set; }

    public decimal TotalSgst { get; set; }

    public decimal TotalIgst { get; set; }

    // ========================================================
    // SHIPPING / DISCOUNT
    // ========================================================

    public decimal ShippingCharge { get; set; }

    public decimal DiscountAmount { get; set; }

    // ========================================================
    // FINAL
    // ========================================================

    public decimal TotalAmount { get; set; }

    public bool IsInterState { get; set; }

    public bool IsFreeShipping =>
        ShippingCharge == 0;

    public string ShippingState { get; set; } =
        string.Empty;

    public string ShippingStateCode { get; set; } =
        string.Empty;

    public string TaxType =>
        IsInterState
            ? "IGST"
            : "CGST + SGST";
}

public class CheckoutPreviewItemDto
{
    public Guid ProductId { get; set; }

    public string ProductName { get; set; } =
        string.Empty;

    public string SKU { get; set; } =
        string.Empty;

    public string HsnCode { get; set; } =
        string.Empty;

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal GstRate { get; set; }

    public decimal TaxableAmount { get; set; }

    public decimal GstAmount { get; set; }

    public decimal CgstAmount { get; set; }

    public decimal SgstAmount { get; set; }

    public decimal IgstAmount { get; set; }

    public decimal TotalPrice { get; set; }
}