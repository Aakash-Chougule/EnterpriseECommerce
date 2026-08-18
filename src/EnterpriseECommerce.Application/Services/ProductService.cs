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
/// - Increase inventory stock
/// - Decrease inventory stock
/// - Soft-deactivate products
/// - Map Product entities to ProductDto
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
    // New SKU
    //     → Create product
    //
    // Existing active SKU
    //     → Reject duplicate
    //
    // Existing inactive SKU
    //     → Reactivate existing product
    // ============================================================

    public async Task<ProductDto> CreateProductAsync(
        CreateProductRequest request)
    {
        ArgumentNullException.ThrowIfNull(
            request);

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

        var existingProduct =
            await _productRepository
                .GetBySkuAsync(
                    normalizedSku);

        if (existingProduct is not null)
        {
            if (existingProduct.IsActive)
            {
                throw new InvalidOperationException(
                    "A product with this SKU already exists.");
            }

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

        if (product is null ||
            !product.IsActive)
        {
            return null;
        }

        product.UpdateDetails(
            request.Name,
            request.Description);

        product.UpdatePrice(
            request.Price);

        product.UpdateStock(
            request.StockQuantity);

        await _productRepository
            .UpdateAsync(product);

        return MapToDto(product);
    }

    // ============================================================
    // ADMIN - INCREASE STOCK
    // ============================================================
    //
    // Adds inventory to an active product.
    //
    // Example:
    //
    // Current stock = 10
    // Quantity      = 5
    //
    // New stock     = 15
    // ============================================================

    public async Task<ProductDto>
        IncreaseProductStockAsync(
            Guid productId,
            int quantity)
    {
        if (productId == Guid.Empty)
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
                .GetByIdAsync(productId);

        if (product is null ||
            !product.IsActive)
        {
            throw new KeyNotFoundException(
                "Product not found.");
        }

        product.IncreaseStock(
            quantity);

        await _productRepository
            .UpdateAsync(product);

        return MapToDto(product);
    }

    // ============================================================
    // ADMIN - DECREASE STOCK
    // ============================================================
    //
    // Removes inventory from an active product.
    //
    // Product.ReduceStock() prevents stock from becoming
    // negative.
    // ============================================================

    public async Task<ProductDto>
        DecreaseProductStockAsync(
            Guid productId,
            int quantity)
    {
        if (productId == Guid.Empty)
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
                .GetByIdAsync(productId);

        if (product is null ||
            !product.IsActive)
        {
            throw new KeyNotFoundException(
                "Product not found.");
        }

        // Product domain method checks whether enough
        // stock is available.
        product.ReduceStock(
            quantity);

        await _productRepository
            .UpdateAsync(product);

        return MapToDto(product);
    }

    // ============================================================
    // DEACTIVATE PRODUCT
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
    // Returns active + inactive products.
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
            .Where(product =>
                product.IsActive &&
                product.StockQuantity <= threshold)
            .OrderBy(product =>
                product.StockQuantity)
            .Select(MapToDto)
            .ToList();
    }

    // ============================================================
    // ENTITY → DTO
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