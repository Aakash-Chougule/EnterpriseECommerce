namespace EnterpriseECommerce.Application.DTOs;

/// <summary>
/// Represents the data that can be changed when updating a product.
/// </summary>
public class UpdateProductRequest
{
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public int StockQuantity { get; set; }
}