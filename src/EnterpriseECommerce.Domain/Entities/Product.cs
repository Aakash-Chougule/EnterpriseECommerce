namespace EnterpriseECommerce.Domain.Entities;

public class Product
{
    public Guid Id { get; private set; }

    public Guid CategoryId { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public string SKU { get; private set; } = string.Empty;

    public decimal Price { get; private set; }

    public int StockQuantity { get; private set; }

    public bool IsActive { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime? UpdatedAt { get; private set; }

    private Product()
    {
    }

    public Product(
        Guid categoryId,
        string name,
        string description,
        string sku,
        decimal price,
        int stockQuantity)
    {
        if (price < 0)
            throw new ArgumentException("Price cannot be negative.");

        if (stockQuantity < 0)
            throw new ArgumentException("Stock quantity cannot be negative.");

        Id = Guid.NewGuid();
        CategoryId = categoryId;
        Name = name;
        Description = description;
        SKU = sku;
        Price = price;
        StockQuantity = stockQuantity;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }

    public void UpdatePrice(decimal price)
    {
        if (price < 0)
            throw new ArgumentException("Price cannot be negative.");

        Price = price;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateStock(int quantity)
    {
        if (quantity < 0)
            throw new ArgumentException("Stock quantity cannot be negative.");

        StockQuantity = quantity;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
    public void UpdateDetails(
    string name,
    string description)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException(
                "Product name is required.");

        Name = name.Trim();
        Description = description?.Trim() ?? string.Empty;

        UpdatedAt = DateTime.UtcNow;
    }

    public void ReduceStock(int quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentException(
                "Quantity must be greater than zero.");
        }

        if (quantity > StockQuantity)
        {
            throw new InvalidOperationException(
                "Insufficient stock.");
        }

        StockQuantity -= quantity;
        UpdatedAt = DateTime.UtcNow;
    }
}