namespace EnterpriseECommerce.Application.DTOs;

/// <summary>
/// Data Transfer Object used to expose product information
/// to the API layer.
///
/// DTOs prevent domain entities from being exposed directly
/// through API responses.
/// </summary>
public class ProductDto
{
    /// <summary>
    /// Unique identifier of the product.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Identifier of the category to which the product belongs.
    /// </summary>
    public Guid CategoryId { get; set; }

    /// <summary>
    /// Product display name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Product description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Stock Keeping Unit.
    ///
    /// SKU should uniquely identify the product from a
    /// business perspective.
    /// </summary>
    public string SKU { get; set; } = string.Empty;

    /// <summary>
    /// Current selling price.
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Number of units currently available in inventory.
    /// </summary>
    public int StockQuantity { get; set; }

    /// <summary>
    /// Indicates whether the product is currently active.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// UTC timestamp when the product was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// UTC timestamp when the product was last updated.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}