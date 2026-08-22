namespace EnterpriseECommerce.Application.DTOs;

// ============================================================
// INVENTORY REPORT
// ============================================================

public class InventoryReportDto
{
    public int Threshold { get; set; }

    // ========================================================
    // SUMMARY
    // ========================================================

    public int TotalProducts { get; set; }

    public int ActiveProducts { get; set; }

    public int InactiveProducts { get; set; }

    public int TotalUnits { get; set; }

    public decimal TotalInventoryValue { get; set; }

    public int InStockProducts { get; set; }

    public int LowStockProducts { get; set; }

    public int OutOfStockProducts { get; set; }

    // ========================================================
    // PRODUCT-WISE REPORT
    // ========================================================

    public List<InventoryReportItemDto>
        Products
    { get; set; } = [];

    // ========================================================
    // CATEGORY SUMMARY
    // ========================================================

    public List<CategoryInventorySummaryDto>
        Categories
    { get; set; } = [];
}

// ============================================================
// PRODUCT-WISE INVENTORY ITEM
// ============================================================

public class InventoryReportItemDto
{
    public Guid ProductId { get; set; }

    public string ProductName { get; set; } =
        string.Empty;

    public string SKU { get; set; } =
        string.Empty;

    public Guid CategoryId { get; set; }

    public string CategoryName { get; set; } =
        string.Empty;

    public decimal UnitPrice { get; set; }

    public int StockQuantity { get; set; }

    public decimal StockValue { get; set; }

    public string StockStatus { get; set; } =
        string.Empty;

    public bool IsActive { get; set; }
}

// ============================================================
// CATEGORY SUMMARY
// ============================================================

public class CategoryInventorySummaryDto
{
    public Guid CategoryId { get; set; }

    public string CategoryName { get; set; } =
        string.Empty;

    public int ProductCount { get; set; }

    public int TotalUnits { get; set; }

    public decimal InventoryValue { get; set; }

    public int InStockProducts { get; set; }

    public int LowStockProducts { get; set; }

    public int OutOfStockProducts { get; set; }
}