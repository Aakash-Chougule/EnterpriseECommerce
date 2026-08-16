using EnterpriseECommerce.Application.DTOs;
using EnterpriseECommerce.Application.Services;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseECommerce.API.Controllers;

/// <summary>
/// Provides HTTP endpoints for product management.
///
/// The controller is responsible only for:
/// - Receiving HTTP requests
/// - Calling the Application service
/// - Returning appropriate HTTP responses
///
/// Business logic remains inside ProductService.
/// Database access remains inside the repository.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly ProductService _productService;

    /// <summary>
    /// Creates a new ProductsController.
    /// </summary>
    public ProductsController(ProductService productService)
    {
        _productService = productService;
    }

    // ------------------------------------------------------------
    // GET: api/Products
    // ------------------------------------------------------------
    // Returns all active products.
    //
    // Authentication is not required yet for this endpoint.
    // We can add authorization rules later.
    // ------------------------------------------------------------

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IReadOnlyList<ProductDto>>> GetAll()
    {
        var products = await _productService.GetAllProductsAsync();

        return Ok(products);
    }

    // ------------------------------------------------------------
    // GET: api/Products/{id}
    // ------------------------------------------------------------
    // Returns a single active product.
    // ------------------------------------------------------------

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<ProductDto>> GetById(Guid id)
    {
        var product = await _productService.GetProductByIdAsync(id);

        if (product is null)
        {
            return NotFound(new
            {
                message = "Product not found."
            });
        }

        return Ok(product);
    }
    //
    // ------------------------------------------------------------
    // POST: api/Products
    // ------------------------------------------------------------
    // Creates a new product.
    //
    // Only Admin users are allowed to create products.
    //
    // Requirements:
    // 1. Valid JWT
    // 2. User must have the Admin role
    // ------------------------------------------------------------

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<ProductDto>> Create(
        [FromBody] CreateProductRequest request)
    {
        try
        {
            var product = await _productService.CreateProductAsync(request);

            // Returns HTTP 201 Created.
            //
            // CreatedAtAction also provides the client with the URL
            // where the newly-created product can be retrieved.
            return CreatedAtAction(
                nameof(GetById),
                new { id = product.Id },
                product);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }

    //// ------------------------------------------------------------
    // PUT: api/Products/{id}
    // ------------------------------------------------------------
    // Updates an existing active product.
    //
    // Only Admin users are allowed to update products.
    // ------------------------------------------------------------

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ProductDto>> Update(
        Guid id,
        [FromBody] UpdateProductRequest request)
    {
        try
        {
            var product = await _productService.UpdateProductAsync(
                id,
                request);

            if (product is null)
            {
                return NotFound(new
                {
                    message = "Product not found."
                });
            }

            return Ok(product);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }

    //
    // ------------------------------------------------------------
    // DELETE: api/Products/{id}
    // ------------------------------------------------------------
    // Soft-deletes (deactivates) an existing product.
    //
    // The product is NOT physically removed from the database.
    // Only Admin users are allowed to deactivate products.
    // ------------------------------------------------------------

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var success = await _productService.DeactivateProductAsync(id);

        if (!success)
        {
            return NotFound(new
            {
                message = "Product not found or already inactive."
            });
        }

        return Ok(new
        {
            message = "Product deactivated successfully."
        });
    }
}