using EnterpriseECommerce.Application.DTOs;
using EnterpriseECommerce.Application.Interfaces;
using EnterpriseECommerce.Domain.Entities;

namespace EnterpriseECommerce.Application.Services;

/// <summary>
/// Contains product-related business logic.
///
/// Controllers should delegate business operations to this service
/// rather than directly accessing repositories or the database.
/// </summary>
public class ProductService
{
    private readonly IProductRepository _productRepository;

    public ProductService(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    /// <summary>
    /// Retrieves all active products and maps them to ProductDto.
    /// </summary>
    public async Task<IReadOnlyList<ProductDto>> GetAllProductsAsync()
    {
        var products = await _productRepository.GetAllAsync();

        return products
            .Where(product => product.IsActive)
            .Select(product => new ProductDto
            {
                Id = product.Id,
                CategoryId = product.CategoryId,
                Name = product.Name,
                Description = product.Description,
                SKU = product.SKU,
                Price = product.Price,
                StockQuantity = product.StockQuantity,
                IsActive = product.IsActive
            })
            .ToList();
    }

    /// <summary>
    /// Retrieves an active product by its unique identifier.
    /// </summary>
    public async Task<ProductDto?> GetProductByIdAsync(Guid id)
    {
        var product = await _productRepository.GetByIdAsync(id);

        // Do not expose inactive products through the normal product API.
        if (product is null || !product.IsActive)
        {
            return null;
        }

        return new ProductDto
        {
            Id = product.Id,
            CategoryId = product.CategoryId,
            Name = product.Name,
            Description = product.Description,
            SKU = product.SKU,
            Price = product.Price,
            StockQuantity = product.StockQuantity,
            IsActive = product.IsActive
        };
    }



    //
    /// <summary>
    /// Creates a new product.
    /// 
    /// The service converts the incoming request into a domain Product
    /// and delegates persistence to the repository.
    /// </summary>
    public async Task<ProductDto> CreateProductAsync(
        CreateProductRequest request)
    {
        // ------------------------------------------------------------
        // Basic application-level validation
        // ------------------------------------------------------------

        if (request.CategoryId == Guid.Empty)
        {
            throw new ArgumentException(
                "CategoryId is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new ArgumentException(
                "Product name is required.");
        }

        if (string.IsNullOrWhiteSpace(request.SKU))
        {
            throw new ArgumentException(
                "SKU is required.");
        }

        // ------------------------------------------------------------
        // Create the domain entity.
        //
        // The Product constructor itself also protects domain rules
        // such as negative price and negative stock.
        // ------------------------------------------------------------

        var product = new Product(
            request.CategoryId,
            request.Name.Trim(),
            request.Description?.Trim() ?? string.Empty,
            request.SKU.Trim(),
            request.Price,
            request.StockQuantity);

        // ------------------------------------------------------------
        // Persist the new product.
        // ------------------------------------------------------------

        await _productRepository.AddAsync(product);

        // ------------------------------------------------------------
        // Convert the domain entity to ProductDto.
        // ------------------------------------------------------------

        return new ProductDto
        {
            Id = product.Id,
            CategoryId = product.CategoryId,
            Name = product.Name,
            Description = product.Description,
            SKU = product.SKU,
            Price = product.Price,
            StockQuantity = product.StockQuantity,
            IsActive = product.IsActive
        };
    }

    //
    /// <summary>
    /// Updates an existing product.
    /// </summary>
    public async Task<ProductDto?> UpdateProductAsync(
        Guid id,
        UpdateProductRequest request)
    {
        var product = await _productRepository.GetByIdAsync(id);

        // Product does not exist or has already been deactivated.
        if (product is null || !product.IsActive)
        {
            return null;
        }

        // ------------------------------------------------------------
        // Update domain values through domain methods.
        // ------------------------------------------------------------

        product.UpdateDetails(
            request.Name,
            request.Description);

        product.UpdatePrice(request.Price);

        product.UpdateStock(request.StockQuantity);

        // ------------------------------------------------------------
        // Persist changes.
        // ------------------------------------------------------------

        await _productRepository.UpdateAsync(product);

        // ------------------------------------------------------------
        // Map entity to DTO.
        // ------------------------------------------------------------

        return new ProductDto
        {
            Id = product.Id,
            CategoryId = product.CategoryId,
            Name = product.Name,
            Description = product.Description,
            SKU = product.SKU,
            Price = product.Price,
            StockQuantity = product.StockQuantity,
            IsActive = product.IsActive
        };
    }

    //
    /// <summary>
    /// Deactivates an existing product instead of physically deleting it.
    ///
    /// This is a soft delete. The product remains in the database,
    /// but it will no longer appear in the normal product API.
    /// </summary>
    public async Task<bool> DeactivateProductAsync(Guid id)
    {
        var product = await _productRepository.GetByIdAsync(id);

        // Product doesn't exist or is already inactive.
        if (product is null || !product.IsActive)
        {
            return false;
        }

        // Use the domain method instead of directly changing IsActive.
        product.Deactivate();

        // Persist the change.
        await _productRepository.UpdateAsync(product);

        return true;
    }
}