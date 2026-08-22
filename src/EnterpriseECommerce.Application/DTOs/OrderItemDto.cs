namespace EnterpriseECommerce.Application.DTOs;

public class OrderItemDto
{
    public Guid Id { get; set; }

    public Guid ProductId { get; set; }

    public string ProductName { get; set; } =
        string.Empty;

    public string SKU { get; set; } =
        string.Empty;

    public string HsnCode { get; set; } =
        string.Empty;

    public int Quantity { get; set; }

    // GST-inclusive unit selling price.
    public decimal UnitPrice { get; set; }

    public decimal GstRate { get; set; }

    public decimal TaxableAmount { get; set; }

    public decimal GstAmount { get; set; }

    public decimal CgstAmount { get; set; }

    public decimal SgstAmount { get; set; }

    public decimal IgstAmount { get; set; }

    public decimal TotalPrice { get; set; }
}