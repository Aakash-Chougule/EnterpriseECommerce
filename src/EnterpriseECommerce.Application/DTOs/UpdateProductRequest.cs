namespace EnterpriseECommerce.Application.DTOs;

public class UpdateProductRequest
{
    public string Name { get; set; } =
        string.Empty;

    public string Description { get; set; } =
        string.Empty;

    public decimal Price { get; set; }

    public int StockQuantity { get; set; }

    public string HsnCode { get; set; } =
        string.Empty;

    public decimal GstRate { get; set; }
}