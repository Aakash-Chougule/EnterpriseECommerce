using EnterpriseECommerce.Application.DTOs;
using EnterpriseECommerce.Application.Services;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseECommerce.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly ProductService _productService;

    public ProductsController(
        ProductService productService)
    {
        _productService = productService;
    }

    // ============================================================
    // GET ACTIVE PRODUCTS
    // ============================================================
    //
    // GET: api/Products
    //
    // Public endpoint.
    // Returns active products only.
    // ============================================================

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IReadOnlyList<ProductDto>>>
        GetAll()
    {
        var products =
            await _productService
                .GetAllProductsAsync();

        return Ok(products);
    }

    // ============================================================
    // ADMIN - GET ALL PRODUCTS
    // ============================================================
    //
    // GET: api/Products/admin/all
    //
    // Returns:
    // - Active products
    // - Inactive products
    //
    // Admin only.
    // ============================================================

    [Authorize(Roles = "Admin")]
    [HttpGet("admin/all")]
    public async Task<ActionResult<IReadOnlyList<ProductDto>>>
        GetAllForAdmin()
    {
        var products =
            await _productService
                .GetAllProductsForAdminAsync();

        return Ok(products);
    }

    // ============================================================
    // ADMIN - GET LOW STOCK PRODUCTS
    // ============================================================
    //
    // GET:
    // api/Products/admin/low-stock
    //
    // Optional:
    // api/Products/admin/low-stock?threshold=10
    //
    // Default threshold:
    // 5
    //
    // Admin only.
    // ============================================================

    [Authorize(Roles = "Admin")]
    [HttpGet("admin/low-stock")]
    public async Task<ActionResult<IReadOnlyList<ProductDto>>>
        GetLowStockProducts(
            [FromQuery] int threshold = 5)
    {
        try
        {
            var products =
                await _productService
                    .GetLowStockProductsAsync(
                        threshold);

            return Ok(products);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }

    // ============================================================
    // GET PRODUCT BY ID
    // ============================================================
    //
    // GET:
    // api/Products/{id}
    //
    // Public endpoint.
    // Returns active product only.
    // ============================================================

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<ProductDto>>
        GetById(
            Guid id)
    {
        var product =
            await _productService
                .GetProductByIdAsync(id);

        if (product is null)
        {
            return NotFound(new
            {
                message =
                    "Product not found."
            });
        }

        return Ok(product);
    }

    // ============================================================
    // CREATE PRODUCT
    // ============================================================
    //
    // POST:
    // api/Products
    //
    // Admin only.
    //
    // Handles:
    // - New product creation
    // - Duplicate active SKU rejection
    // - Inactive product reactivation
    // ============================================================

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<ProductDto>>
        Create(
            [FromBody]
            CreateProductRequest request)
    {
        try
        {
            var product =
                await _productService
                    .CreateProductAsync(
                        request);

            return CreatedAtAction(
                nameof(GetById),
                new
                {
                    id = product.Id
                },
                product);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new
            {
                message = ex.Message
            });
        }
    }

    // ============================================================
    // UPDATE PRODUCT
    // ============================================================
    //
    // PUT:
    // api/Products/{id}
    //
    // Admin only.
    // ============================================================

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ProductDto>>
        Update(
            Guid id,
            [FromBody]
            UpdateProductRequest request)
    {
        try
        {
            var product =
                await _productService
                    .UpdateProductAsync(
                        id,
                        request);

            if (product is null)
            {
                return NotFound(new
                {
                    message =
                        "Product not found."
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

    // ============================================================
    // ADMIN - INCREASE PRODUCT STOCK
    // ============================================================
    //
    // POST:
    // api/Products/{id}/stock/increase
    //
    // Request:
    //
    // {
    //     "quantity": 10
    // }
    //
    // Example:
    //
    // Current stock = 5
    // Add           = 10
    // New stock     = 15
    //
    // Admin only.
    // ============================================================

    [Authorize(Roles = "Admin")]
    [HttpPost("{id:guid}/stock/increase")]
    public async Task<ActionResult<ProductDto>>
        IncreaseStock(
            Guid id,
            [FromBody]
            StockAdjustmentRequest request)
    {
        try
        {
            var product =
                await _productService
                    .IncreaseProductStockAsync(
                        id,
                        request.Quantity);

            return Ok(product);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new
            {
                message = ex.Message
            });
        }
    }

    // ============================================================
    // ADMIN - DECREASE PRODUCT STOCK
    // ============================================================
    //
    // POST:
    // api/Products/{id}/stock/decrease
    //
    // Request:
    //
    // {
    //     "quantity": 5
    // }
    //
    // Product.ReduceStock() prevents stock from becoming
    // negative.
    //
    // Admin only.
    // ============================================================

    [Authorize(Roles = "Admin")]
    [HttpPost("{id:guid}/stock/decrease")]
    public async Task<ActionResult<ProductDto>>
        DecreaseStock(
            Guid id,
            [FromBody]
            StockAdjustmentRequest request)
    {
        try
        {
            var product =
                await _productService
                    .DecreaseProductStockAsync(
                        id,
                        request.Quantity);

            return Ok(product);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new
            {
                message = ex.Message
            });
        }
    }

    // ============================================================
    // DEACTIVATE PRODUCT
    // ============================================================
    //
    // DELETE:
    // api/Products/{id}
    //
    // This is a soft delete.
    //
    // Admin only.
    // ============================================================

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult>
        Delete(
            Guid id)
    {
        var success =
            await _productService
                .DeactivateProductAsync(id);

        if (!success)
        {
            return NotFound(new
            {
                message =
                    "Product not found or already inactive."
            });
        }

        return Ok(new
        {
            message =
                "Product deactivated successfully."
        });
    }
}

// ============================================================
// STOCK ADJUSTMENT REQUEST
// ============================================================
//
// Request body used by:
//
// POST /api/Products/{id}/stock/increase
// POST /api/Products/{id}/stock/decrease
//
// Example:
//
// {
//     "quantity": 5
// }
// ============================================================

public class StockAdjustmentRequest
{
    public int Quantity { get; set; }
}