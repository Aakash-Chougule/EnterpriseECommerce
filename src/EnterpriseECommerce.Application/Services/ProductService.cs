using EnterpriseECommerce.Application.DTOs;
using EnterpriseECommerce.Application.Interfaces;
using EnterpriseECommerce.Domain.Entities;

namespace EnterpriseECommerce.Application.Services;

/// <summary>
/// Contains product-related business logic.
///
/// Responsibilities:
/// - Retrieve active products
/// - Create new products
/// - Reactivate inactive products with the same SKU
/// - Update products
/// - Soft-deactivate products
/// - Map Product entities to ProductDto
///
/// Controllers should delegate business operations to this service
/// rather than directly accessing repositories or the database.
/// </summary>
public class ProductService
{
    private readonly IProductRepository _productRepository;

    public ProductService(
        IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    // ============================================================
    // GET ALL ACTIVE PRODUCTS
    // ============================================================

    public async Task<IReadOnlyList<ProductDto>>
        GetAllProductsAsync()
    {
        var products =
            await _productRepository
                .GetAllAsync();

        return products
            .Where(product =>
                product.IsActive)
            .Select(MapToDto)
            .ToList();
    }

    // ============================================================
    // GET PRODUCT BY ID
    // ============================================================

    public async Task<ProductDto?>
        GetProductByIdAsync(
            Guid id)
    {
        if (id == Guid.Empty)
        {
            return null;
        }

        var product =
            await _productRepository
                .GetByIdAsync(id);

        // Normal customer/product API should not expose
        // inactive products.
        if (product is null ||
            !product.IsActive)
        {
            return null;
        }

        return MapToDto(product);
    }

    // ============================================================
    // CREATE PRODUCT
    // ============================================================
    //
    // Behavior:
    //
    // New SKU
    //     → Create new product.
    //
    // Existing ACTIVE SKU
    //     → Reject duplicate.
    //
    // Existing INACTIVE SKU
    //     → Restore/reactivate old product instead of
    //       creating a duplicate database row.
    // ============================================================

    public async Task<ProductDto> CreateProductAsync(
        CreateProductRequest request)
    {
        ArgumentNullException.ThrowIfNull(
            request);

        // --------------------------------------------------------
        // Validation
        // --------------------------------------------------------

        if (request.CategoryId ==
            Guid.Empty)
        {
            throw new ArgumentException(
                "CategoryId is required.");
        }

        if (string.IsNullOrWhiteSpace(
            request.Name))
        {
            throw new ArgumentException(
                "Product name is required.");
        }

        if (string.IsNullOrWhiteSpace(
            request.SKU))
        {
            throw new ArgumentException(
                "SKU is required.");
        }

        if (request.Price < 0)
        {
            throw new ArgumentException(
                "Price cannot be negative.");
        }

        if (request.StockQuantity < 0)
        {
            throw new ArgumentException(
                "Stock quantity cannot be negative.");
        }

        var normalizedName =
            request.Name.Trim();

        var normalizedDescription =
            request.Description?.Trim()
            ?? string.Empty;

        var normalizedSku =
            request.SKU.Trim();

        // --------------------------------------------------------
        // Check whether SKU already exists.
        //
        // GetBySkuAsync must return both active and inactive
        // products.
        // --------------------------------------------------------

        var existingProduct =
            await _productRepository
                .GetBySkuAsync(
                    normalizedSku);

        if (existingProduct is not null)
        {
            // ----------------------------------------------------
            // Same SKU already belongs to an ACTIVE product.
            // ----------------------------------------------------

            if (existingProduct.IsActive)
            {
                throw new InvalidOperationException(
                    "A product with this SKU already exists.");
            }

            // ----------------------------------------------------
            // Same SKU exists but product is INACTIVE.
            //
            // Restore the existing record.
            // ----------------------------------------------------

            existingProduct.UpdateCategory(
                request.CategoryId);

            existingProduct.UpdateDetails(
                normalizedName,
                normalizedDescription);

            existingProduct.UpdatePrice(
                request.Price);

            existingProduct.UpdateStock(
                request.StockQuantity);

            existingProduct.Activate();

            await _productRepository
                .UpdateAsync(
                    existingProduct);

            return MapToDto(
                existingProduct);
        }

        // --------------------------------------------------------
        // Completely new SKU.
        // --------------------------------------------------------

        var product =
            new Product(
                request.CategoryId,
                normalizedName,
                normalizedDescription,
                normalizedSku,
                request.Price,
                request.StockQuantity);

        await _productRepository
            .AddAsync(product);

        return MapToDto(product);
    }

    // ============================================================
    // UPDATE PRODUCT
    // ============================================================

    public async Task<ProductDto?>
        UpdateProductAsync(
            Guid id,
            UpdateProductRequest request)
    {
        ArgumentNullException.ThrowIfNull(
            request);

        if (id == Guid.Empty)
        {
            return null;
        }

        var product =
            await _productRepository
                .GetByIdAsync(id);

        // Product does not exist or has been deactivated.
        if (product is null ||
            !product.IsActive)
        {
            return null;
        }

        // --------------------------------------------------------
        // Domain methods enforce product rules.
        // --------------------------------------------------------

        product.UpdateDetails(
            request.Name,
            request.Description);

        product.UpdatePrice(
            request.Price);

        product.UpdateStock(
            request.StockQuantity);

        // --------------------------------------------------------
        // Persist changes.
        // --------------------------------------------------------

        await _productRepository
            .UpdateAsync(product);

        return MapToDto(product);
    }

    // ============================================================
    // DEACTIVATE PRODUCT
    // ============================================================
    //
    // Soft delete:
    //
    // IsActive = false
    //
    // Product remains in PostgreSQL so historical order data and
    // references remain valid.
    // ============================================================

    public async Task<bool>
        DeactivateProductAsync(
            Guid id)
    {
        if (id == Guid.Empty)
        {
            return false;
        }

        var product =
            await _productRepository
                .GetByIdAsync(id);

        if (product is null ||
            !product.IsActive)
        {
            return false;
        }

        product.Deactivate();

        await _productRepository
            .UpdateAsync(product);

        return true;
    }
    // ============================================================
    // ADMIN - GET ALL PRODUCTS
    // ============================================================
    //
    // Returns both active and inactive products.
    // ============================================================

    public async Task<IReadOnlyList<ProductDto>>
        GetAllProductsForAdminAsync()
    {
        var products =
            await _productRepository
                .GetAllAsync();

        return products
            .Select(MapToDto)
            .ToList();
    }

    // ============================================================
    // ENTITY → DTO MAPPING
    // ============================================================

    private static ProductDto MapToDto(
        Product product)
    {
        return new ProductDto
        {
            Id =
                product.Id,

            CategoryId =
                product.CategoryId,

            Name =
                product.Name,

            Description =
                product.Description,

            SKU =
                product.SKU,

            Price =
                product.Price,

            StockQuantity =
                product.StockQuantity,

            IsActive =
                product.IsActive,

            CreatedAt =
                product.CreatedAt,

            UpdatedAt =
                product.UpdatedAt
        };
    }
}