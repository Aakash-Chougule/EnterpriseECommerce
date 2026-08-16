namespace EnterpriseECommerce.Application.DTOs;

/// <summary>
/// Represents the data required to create a new product.
///
/// System-generated properties such as Id, IsActive and CreatedAt
/// are intentionally not included because those values are controlled
/// by the application/domain layer.
/// </summary>
public class CreateProductRequest
{
    /// <summary>
    /// Category to which the product belongs.
    /// </summary>
    public Guid CategoryId { get; set; }

    /// <summary>
    /// Product name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Product description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Stock Keeping Unit.
    /// </summary>
    public string SKU { get; set; } = string.Empty;

    /// <summary>
    /// Product selling price.
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Initial quantity available in stock.
    /// </summary>
    public int StockQuantity { get; set; }
}
