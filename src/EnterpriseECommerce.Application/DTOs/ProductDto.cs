namespace EnterpriseECommerce.Application.DTOs;

public class ProductDto
{
    public Guid Id { get; set; }

    public Guid CategoryId { get; set; }

    public string Name { get; set; } =
        string.Empty;

    public string Description { get; set; } =
        string.Empty;

    public string SKU { get; set; } =
        string.Empty;

    // ========================================================
    // GST
    // ========================================================

    public string HsnCode { get; set; } =
        string.Empty;

    public decimal GstRate { get; set; }

    // ========================================================
    // PRICE
    // ========================================================

    /// <summary>
    /// GST-inclusive selling price.
    /// </summary>
    public decimal Price { get; set; }

    public int StockQuantity { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}