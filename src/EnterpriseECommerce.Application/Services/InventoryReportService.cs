using EnterpriseECommerce.Application.DTOs;
using EnterpriseECommerce.Application.Interfaces;

namespace EnterpriseECommerce.Application.Services;

public class InventoryReportService
{
    private readonly IProductRepository
        _productRepository;

    public InventoryReportService(
        IProductRepository productRepository)
    {
        _productRepository =
            productRepository;
    }

    // ========================================================
    // GET INVENTORY REPORT
    // ========================================================

    public async Task<InventoryReportDto>
        GetInventoryReportAsync(
            int threshold = 5)
    {
        if (threshold < 0)
        {
            throw new ArgumentException(
                "Low stock threshold cannot be negative.");
        }

        var products =
            (
                await _productRepository
                    .GetInventoryReportItemsAsync()
            )
            .ToList();

        // ====================================================
        // CALCULATE PRODUCT VALUES
        // ====================================================

        foreach (var product in products)
        {
            product.StockValue =
                product.UnitPrice *
                product.StockQuantity;

            product.StockStatus =
                GetStockStatus(
                    product.StockQuantity,
                    threshold);
        }

        // ====================================================
        // CATEGORY SUMMARY
        // ====================================================

        var categories =
            products
                .GroupBy(
                    product =>
                        new
                        {
                            product.CategoryId,
                            product.CategoryName
                        })
                .Select(
                    group =>
                        new CategoryInventorySummaryDto
                        {
                            CategoryId =
                                group.Key.CategoryId,

                            CategoryName =
                                group.Key.CategoryName,

                            ProductCount =
                                group.Count(),

                            TotalUnits =
                                group.Sum(
                                    product =>
                                        product.StockQuantity),

                            InventoryValue =
                                group.Sum(
                                    product =>
                                        product.StockValue),

                            InStockProducts =
                                group.Count(
                                    product =>
                                        product.StockQuantity >
                                        threshold),

                            LowStockProducts =
                                group.Count(
                                    product =>
                                        product.StockQuantity > 0 &&
                                        product.StockQuantity <=
                                        threshold),

                            OutOfStockProducts =
                                group.Count(
                                    product =>
                                        product.StockQuantity == 0)
                        })
                .OrderBy(
                    item =>
                        item.CategoryName)
                .ToList();

        // ====================================================
        // RETURN
        // ====================================================

        return new InventoryReportDto
        {
            Threshold =
                threshold,

            TotalProducts =
                products.Count,

            ActiveProducts =
                products.Count(
                    product =>
                        product.IsActive),

            InactiveProducts =
                products.Count(
                    product =>
                        !product.IsActive),

            TotalUnits =
                products.Sum(
                    product =>
                        product.StockQuantity),

            TotalInventoryValue =
                products.Sum(
                    product =>
                        product.StockValue),

            InStockProducts =
                products.Count(
                    product =>
                        product.StockQuantity >
                        threshold),

            LowStockProducts =
                products.Count(
                    product =>
                        product.StockQuantity > 0 &&
                        product.StockQuantity <=
                        threshold),

            OutOfStockProducts =
                products.Count(
                    product =>
                        product.StockQuantity == 0),

            Products =
                products
                    .OrderBy(
                        product =>
                            product.CategoryName)
                    .ThenBy(
                        product =>
                            product.ProductName)
                    .ToList(),

            Categories =
                categories
        };
    }

    // ========================================================
    // STATUS
    // ========================================================

    private static string GetStockStatus(
        int quantity,
        int threshold)
    {
        if (quantity == 0)
        {
            return
                "Out of Stock";
        }

        if (quantity <= threshold)
        {
            return
                "Low Stock";
        }

        return
            "In Stock";
    }
}