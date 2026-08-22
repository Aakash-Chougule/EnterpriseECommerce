using EnterpriseECommerce.Application.DTOs;
using EnterpriseECommerce.Application.Interfaces;
using EnterpriseECommerce.Domain.Entities;

namespace EnterpriseECommerce.Application.Services;

/// <summary>
/// Contains product-related business logic.
///
/// Responsibilities:
/// - Retrieve active products
/// - Retrieve all products for Admin
/// - Retrieve low-stock products
/// - Create products
/// - Reactivate inactive products with the same SKU
/// - Update products
/// - Manage GST / HSN information
/// - Increase inventory stock
/// - Decrease inventory stock
/// - Soft-deactivate products
/// - Map Product entities to ProductDto
/// </summary>
public class ProductService
{
    private readonly IProductRepository
        _productRepository;

    public ProductService(
        IProductRepository productRepository)
    {
        _productRepository =
            productRepository;
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
            .Where(
                product =>
                    product.IsActive)
            .Select(
                MapToDto)
            .ToList();
    }

    // ============================================================
    // GET PRODUCT BY ID
    // ============================================================

    public async Task<ProductDto?>
        GetProductByIdAsync(
            Guid id)
    {
        if (id ==
            Guid.Empty)
        {
            return null;
        }

        var product =
            await _productRepository
                .GetByIdAsync(
                    id);

        if (product is null ||
            !product.IsActive)
        {
            return null;
        }

        return MapToDto(
            product);
    }

    // ============================================================
    // CREATE PRODUCT
    // ============================================================

    public async Task<ProductDto>
        CreateProductAsync(
            CreateProductRequest request)
    {
        ArgumentNullException.ThrowIfNull(
            request);

        // ========================================================
        // VALIDATION
        // ========================================================

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

        ValidateGstRate(
            request.GstRate);

        // HSN is allowed to remain blank temporarily.
        // Existing products migrated from the previous model
        // will also initially have an empty HSN code.

        var normalizedName =
            request.Name.Trim();

        var normalizedDescription =
            request.Description?.Trim()
            ?? string.Empty;

        var normalizedSku =
            request.SKU
                .Trim()
                .ToUpperInvariant();

        var normalizedHsn =
            request.HsnCode?.Trim()
            ?? string.Empty;

        // ========================================================
        // CHECK EXISTING SKU
        // ========================================================

        var existingProduct =
            await _productRepository
                .GetBySkuAsync(
                    normalizedSku);

        if (existingProduct is not null)
        {
            // ====================================================
            // ACTIVE DUPLICATE
            // ====================================================

            if (existingProduct.IsActive)
            {
                throw new InvalidOperationException(
                    "A product with this SKU already exists.");
            }

            // ====================================================
            // REACTIVATE PREVIOUS PRODUCT
            // ====================================================

            existingProduct.UpdateCategory(
                request.CategoryId);

            existingProduct.UpdateDetails(
                normalizedName,
                normalizedDescription);

            existingProduct.UpdatePrice(
                request.Price);

            existingProduct.UpdateStock(
                request.StockQuantity);

            existingProduct.UpdateTaxInformation(
                normalizedHsn,
                request.GstRate);

            existingProduct.Activate();

            await _productRepository
                .UpdateAsync(
                    existingProduct);

            return MapToDto(
                existingProduct);
        }

        // ========================================================
        // CREATE NEW PRODUCT
        // ========================================================

        var product =
            new Product(
                categoryId:
                    request.CategoryId,

                name:
                    normalizedName,

                description:
                    normalizedDescription,

                sku:
                    normalizedSku,

                price:
                    request.Price,

                stockQuantity:
                    request.StockQuantity,

                hsnCode:
                    normalizedHsn,

                gstRate:
                    request.GstRate);

        await _productRepository
            .AddAsync(
                product);

        return MapToDto(
            product);
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

        if (id ==
            Guid.Empty)
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(
            request.Name))
        {
            throw new ArgumentException(
                "Product name is required.");
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

        ValidateGstRate(
            request.GstRate);

        var product =
            await _productRepository
                .GetByIdAsync(
                    id);

        if (product is null ||
            !product.IsActive)
        {
            return null;
        }

        product.UpdateDetails(
            request.Name.Trim(),
            request.Description?.Trim()
            ?? string.Empty);

        product.UpdatePrice(
            request.Price);

        product.UpdateStock(
            request.StockQuantity);

        product.UpdateTaxInformation(
            request.HsnCode,
            request.GstRate);

        await _productRepository
            .UpdateAsync(
                product);

        return MapToDto(
            product);
    }

    // ============================================================
    // ADMIN - INCREASE STOCK
    // ============================================================

    public async Task<ProductDto>
        IncreaseProductStockAsync(
            Guid productId,
            int quantity)
    {
        if (productId ==
            Guid.Empty)
        {
            throw new ArgumentException(
                "ProductId is required.");
        }

        if (quantity <= 0)
        {
            throw new ArgumentException(
                "Quantity must be greater than zero.");
        }

        var product =
            await _productRepository
                .GetByIdAsync(
                    productId);

        if (product is null ||
            !product.IsActive)
        {
            throw new KeyNotFoundException(
                "Product not found.");
        }

        product.IncreaseStock(
            quantity);

        await _productRepository
            .UpdateAsync(
                product);

        return MapToDto(
            product);
    }

    // ============================================================
    // ADMIN - DECREASE STOCK
    // ============================================================

    public async Task<ProductDto>
        DecreaseProductStockAsync(
            Guid productId,
            int quantity)
    {
        if (productId ==
            Guid.Empty)
        {
            throw new ArgumentException(
                "ProductId is required.");
        }

        if (quantity <= 0)
        {
            throw new ArgumentException(
                "Quantity must be greater than zero.");
        }

        var product =
            await _productRepository
                .GetByIdAsync(
                    productId);

        if (product is null ||
            !product.IsActive)
        {
            throw new KeyNotFoundException(
                "Product not found.");
        }

        product.ReduceStock(
            quantity);

        await _productRepository
            .UpdateAsync(
                product);

        return MapToDto(
            product);
    }

    // ============================================================
    // DEACTIVATE PRODUCT
    // ============================================================

    public async Task<bool>
        DeactivateProductAsync(
            Guid id)
    {
        if (id ==
            Guid.Empty)
        {
            return false;
        }

        var product =
            await _productRepository
                .GetByIdAsync(
                    id);

        if (product is null ||
            !product.IsActive)
        {
            return false;
        }

        product.Deactivate();

        await _productRepository
            .UpdateAsync(
                product);

        return true;
    }

    // ============================================================
    // ADMIN - GET ALL PRODUCTS
    // ============================================================

    public async Task<IReadOnlyList<ProductDto>>
        GetAllProductsForAdminAsync()
    {
        var products =
            await _productRepository
                .GetAllAsync();

        return products
            .Select(
                MapToDto)
            .ToList();
    }

    // ============================================================
    // ADMIN - GET LOW STOCK PRODUCTS
    // ============================================================

    public async Task<IReadOnlyList<ProductDto>>
        GetLowStockProductsAsync(
            int threshold = 5)
    {
        if (threshold < 0)
        {
            throw new ArgumentException(
                "Low stock threshold cannot be negative.");
        }

        var products =
            await _productRepository
                .GetAllAsync();

        return products
            .Where(
                product =>
                    product.IsActive &&
                    product.StockQuantity <=
                    threshold)
            .OrderBy(
                product =>
                    product.StockQuantity)
            .Select(
                MapToDto)
            .ToList();
    }

    // ============================================================
    // VALIDATE GST
    // ============================================================

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

    // ============================================================
    // ENTITY -> DTO
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

            HsnCode =
                product.HsnCode,

            GstRate =
                product.GstRate,

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