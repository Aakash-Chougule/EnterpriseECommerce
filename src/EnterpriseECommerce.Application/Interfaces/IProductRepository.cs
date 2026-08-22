using EnterpriseECommerce.Application.DTOs;
using EnterpriseECommerce.Domain.Entities;

namespace EnterpriseECommerce.Application.Interfaces;

/// <summary>
/// Defines product repository operations.
/// </summary>
public interface IProductRepository
{
    // ========================================================
    // STANDARD PRODUCT OPERATIONS
    // ========================================================

    Task<Product?> GetByIdAsync(
        Guid id);

    Task<Product?> GetBySkuAsync(
        string sku);

    Task<IReadOnlyList<Product>>
        GetAllAsync();

    Task AddAsync(
        Product product);

    Task UpdateAsync(
        Product product);

    Task DeleteAsync(
        Product product);

    // ========================================================
    // INVENTORY REPORTING
    // ========================================================
    //
    // Returns product information together with its category.
    //
    // StockStatus and StockValue will be calculated by the
    // application service.
    // ========================================================

    Task<IReadOnlyList<InventoryReportItemDto>>
        GetInventoryReportItemsAsync();
}