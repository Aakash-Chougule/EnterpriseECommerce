using EnterpriseECommerce.Application.DTOs;
using EnterpriseECommerce.Application.Interfaces;
using EnterpriseECommerce.Domain.Entities;
using EnterpriseECommerce.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

namespace EnterpriseECommerce.Infrastructure.Repositories;

public class ProductRepository :
    IProductRepository
{
    private readonly AppDbContext
        _context;

    public ProductRepository(
        AppDbContext context)
    {
        _context =
            context;
    }

    // ========================================================
    // GET PRODUCT
    // ========================================================

    public async Task<Product?>
        GetByIdAsync(
            Guid id)
    {
        return await _context
            .Products
            .FirstOrDefaultAsync(
                product =>
                    product.Id == id);
    }

    // ========================================================
    // GET BY SKU
    // ========================================================

    public async Task<Product?>
        GetBySkuAsync(
            string sku)
    {
        return await _context
            .Products
            .FirstOrDefaultAsync(
                product =>
                    product.SKU.ToLower() ==
                    sku.ToLower());
    }

    // ========================================================
    // GET ALL
    // ========================================================

    public async Task<IReadOnlyList<Product>>
        GetAllAsync()
    {
        return await _context
            .Products
            .AsNoTracking()
            .OrderBy(
                product =>
                    product.Name)
            .ToListAsync();
    }

    // ========================================================
    // INVENTORY REPORT
    // ========================================================

    public async Task<
        IReadOnlyList<InventoryReportItemDto>>
        GetInventoryReportItemsAsync()
    {
        var result =
            await (
                from product in
                    _context.Products
                        .AsNoTracking()

                join category in
                    _context.Categories
                        .AsNoTracking()

                on product.CategoryId
                    equals category.Id

                into productCategories

                from category in
                    productCategories
                        .DefaultIfEmpty()

                orderby
                    category != null
                        ? category.Name
                        : string.Empty,
                    product.Name

                select new InventoryReportItemDto
                {
                    ProductId =
                        product.Id,

                    ProductName =
                        product.Name,

                    SKU =
                        product.SKU,

                    CategoryId =
                        product.CategoryId,

                    CategoryName =
                        category != null
                            ? category.Name
                            : "Unknown Category",

                    UnitPrice =
                        product.Price,

                    StockQuantity =
                        product.StockQuantity,

                    IsActive =
                        product.IsActive
                }
            )
            .ToListAsync();

        return result;
    }

    // ========================================================
    // ADD
    // ========================================================

    public async Task AddAsync(
        Product product)
    {
        await _context
            .Products
            .AddAsync(
                product);

        await _context
            .SaveChangesAsync();
    }

    // ========================================================
    // UPDATE
    // ========================================================

    public async Task UpdateAsync(
        Product product)
    {
        _context.Products
            .Update(
                product);

        await _context
            .SaveChangesAsync();
    }

    // ========================================================
    // DELETE
    // ========================================================

    public async Task DeleteAsync(
        Product product)
    {
        _context.Products
            .Remove(
                product);

        await _context
            .SaveChangesAsync();
    }
}