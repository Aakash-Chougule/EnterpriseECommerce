using EnterpriseECommerce.Application.DTOs;
using EnterpriseECommerce.Application.Interfaces;

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
}