namespace EnterpriseECommerce.Domain.Entities;

public class Product
{
    public Guid Id { get; private set; }

    public Guid CategoryId { get; private set; }

    public string Name { get; private set; } =
        string.Empty;

    public string Description { get; private set; } =
        string.Empty;

    public string SKU { get; private set; } =
        string.Empty;

    // ========================================================
    // TAX INFORMATION
    // ========================================================

    /// <summary>
    /// HSN code used for GST invoicing.
    ///
    /// Existing products may temporarily have an empty HSN code
    /// after migration.
    /// </summary>
    public string HsnCode { get; private set; } =
        string.Empty;

    /// <summary>
    /// GST percentage applicable to this product.
    ///
    /// Examples:
    /// 0
    /// 5
    /// 12
    /// 18
    /// 28
    /// </summary>
    public decimal GstRate { get; private set; }

    // ========================================================
    // SELLING PRICE
    // ========================================================
    //
    // IMPORTANT:
    //
    // Price is GST-INCLUSIVE.
    //
    // Example:
    //
    // Price   = ₹1,180
    // GST     = 18%
    //
    // Taxable = ₹1,000
    // GST     = ₹180
    //
    // Customer still sees ₹1,180.
    // ========================================================

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
        int stockQuantity,
        string? hsnCode = null,
        decimal gstRate = 0)
    {
        if (categoryId == Guid.Empty)
        {
            throw new ArgumentException(
                "CategoryId is required.");
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "Product name is required.");
        }

        if (string.IsNullOrWhiteSpace(sku))
        {
            throw new ArgumentException(
                "SKU is required.");
        }

        if (price < 0)
        {
            throw new ArgumentException(
                "Price cannot be negative.");
        }

        if (stockQuantity < 0)
        {
            throw new ArgumentException(
                "Stock quantity cannot be negative.");
        }

        ValidateGstRate(
            gstRate);

        Id =
            Guid.NewGuid();

        CategoryId =
            categoryId;

        Name =
            name.Trim();

        Description =
            description?.Trim() ??
            string.Empty;

        SKU =
            sku.Trim();

        Price =
            price;

        StockQuantity =
            stockQuantity;

        HsnCode =
            hsnCode?.Trim() ??
            string.Empty;

        GstRate =
            gstRate;

        IsActive =
            true;

        CreatedAt =
            DateTime.UtcNow;
    }

    // ========================================================
    // PRICE
    // ========================================================

    public void UpdatePrice(
        decimal price)
    {
        if (price < 0)
        {
            throw new ArgumentException(
                "Price cannot be negative.");
        }

        Price =
            price;

        UpdatedAt =
            DateTime.UtcNow;
    }

    // ========================================================
    // TAX
    // ========================================================

    public void UpdateTaxInformation(
        string? hsnCode,
        decimal gstRate)
    {
        ValidateGstRate(
            gstRate);

        HsnCode =
            hsnCode?.Trim() ??
            string.Empty;

        GstRate =
            gstRate;

        UpdatedAt =
            DateTime.UtcNow;
    }

    // ========================================================
    // STOCK
    // ========================================================

    public void UpdateStock(
        int quantity)
    {
        if (quantity < 0)
        {
            throw new ArgumentException(
                "Stock quantity cannot be negative.");
        }

        StockQuantity =
            quantity;

        UpdatedAt =
            DateTime.UtcNow;
    }

    public void ReduceStock(
        int quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentException(
                "Quantity must be greater than zero.");
        }

        if (quantity >
            StockQuantity)
        {
            throw new InvalidOperationException(
                "Insufficient stock.");
        }

        StockQuantity -=
            quantity;

        UpdatedAt =
            DateTime.UtcNow;
    }

    public void IncreaseStock(
        int quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentException(
                "Quantity must be greater than zero.");
        }

        StockQuantity +=
            quantity;

        UpdatedAt =
            DateTime.UtcNow;
    }

    // ========================================================
    // DETAILS
    // ========================================================

    public void UpdateDetails(
        string name,
        string description)
    {
        if (string.IsNullOrWhiteSpace(
            name))
        {
            throw new ArgumentException(
                "Product name is required.");
        }

        Name =
            name.Trim();

        Description =
            description?.Trim() ??
            string.Empty;

        UpdatedAt =
            DateTime.UtcNow;
    }

    // ========================================================
    // CATEGORY
    // ========================================================

    public void UpdateCategory(
        Guid categoryId)
    {
        if (categoryId ==
            Guid.Empty)
        {
            throw new ArgumentException(
                "CategoryId is required.");
        }

        CategoryId =
            categoryId;

        UpdatedAt =
            DateTime.UtcNow;
    }

    // ========================================================
    // ACTIVATION
    // ========================================================

    public void Deactivate()
    {
        IsActive =
            false;

        UpdatedAt =
            DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive =
            true;

        UpdatedAt =
            DateTime.UtcNow;
    }

    // ========================================================
    // GST VALIDATION
    // ========================================================

    private static void ValidateGstRate(
        decimal gstRate)
    {
        if (gstRate < 0 ||
            gstRate > 100)
        {
            throw new ArgumentException(
                "GST rate must be between 0 and 100.");
        }
    }
}