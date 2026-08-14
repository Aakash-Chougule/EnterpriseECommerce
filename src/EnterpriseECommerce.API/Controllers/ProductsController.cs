using EnterpriseECommerce.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseECommerce.API.Controllers;

/// <summary>
/// Provides endpoints for managing products.
///
/// Base URL:
///     /api/products
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly ProductService _productService;

    public ProductsController(ProductService productService)
    {
        _productService = productService;
    }

    /// <summary>
    /// Returns all active products.
    ///
    /// GET: /api/products
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetProducts()
    {
        var products = await _productService.GetAllProductsAsync();

        return Ok(products);
    }
}