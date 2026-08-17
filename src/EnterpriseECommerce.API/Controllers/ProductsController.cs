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
    // ============================================================

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<
        IReadOnlyList<ProductDto>>> GetAll()
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
    // Returns active + inactive products.
    // ============================================================

    [Authorize(Roles = "Admin")]
    [HttpGet("admin/all")]
    public async Task<ActionResult<
        IReadOnlyList<ProductDto>>> GetAllForAdmin()
    {
        var products =
            await _productService
                .GetAllProductsForAdminAsync();

        return Ok(products);
    }

    // ============================================================
    // GET PRODUCT BY ID
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
                message =
                    ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new
            {
                message =
                    ex.Message
            });
        }
    }

    // ============================================================
    // UPDATE PRODUCT
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
                message =
                    ex.Message
            });
        }
    }

    // ============================================================
    // DEACTIVATE PRODUCT
    // ============================================================

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
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