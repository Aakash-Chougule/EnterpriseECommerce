namespace EnterpriseECommerce.Application.DTOs;

/// <summary>
/// Data Transfer Object used to expose product information
/// through the API without exposing the domain entity directly.
/// </summary>
public class ProductDto
{
    public Guid Id { get; set; }

    public Guid CategoryId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string SKU { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public int StockQuantity { get; set; }

    public bool IsActive { get; set; }
}