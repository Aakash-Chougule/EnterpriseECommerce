using EnterpriseECommerce.Application.Services;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseECommerce.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly CategoryService _categoryService;

    public CategoriesController(
        CategoryService categoryService)
    {
        _categoryService =
            categoryService;
    }

    // ============================================================
    // GET ACTIVE CATEGORIES
    // ============================================================

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var categories =
            await _categoryService
                .GetAllCategoriesAsync();

        return Ok(categories);
    }

    // ============================================================
    // ADMIN - GET ALL CATEGORIES
    // ============================================================
    //
    // GET: api/Categories/admin/all
    //
    // Returns active + inactive categories.
    // ============================================================

    [Authorize(Roles = "Admin")]
    [HttpGet("admin/all")]
    public async Task<IActionResult>
        GetAllForAdmin()
    {
        var categories =
            await _categoryService
                .GetAllCategoriesForAdminAsync();

        return Ok(categories);
    }

    // ============================================================
    // GET CATEGORY BY ID
    // ============================================================

    [AllowAnonymous]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult>
        GetById(
            Guid id)
    {
        var category =
            await _categoryService
                .GetCategoryByIdAsync(id);

        if (category is null)
        {
            return NotFound(new
            {
                message =
                    "Category not found."
            });
        }

        return Ok(category);
    }

    // ============================================================
    // CREATE CATEGORY
    // ============================================================

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult>
        Create(
            [FromBody]
            CreateCategoryRequest request)
    {
        try
        {
            var category =
                await _categoryService
                    .CreateCategoryAsync(
                        request.Name,
                        request.Description);

            return CreatedAtAction(
                nameof(GetById),
                new
                {
                    id = category.Id
                },
                category);
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
    // UPDATE CATEGORY
    // ============================================================

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult>
        Update(
            Guid id,
            [FromBody]
            UpdateCategoryRequest request)
    {
        try
        {
            var category =
                await _categoryService
                    .UpdateCategoryAsync(
                        id,
                        request.Name,
                        request.Description);

            if (category is null)
            {
                return NotFound(new
                {
                    message =
                        "Category not found."
                });
            }

            return Ok(category);
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
    // DEACTIVATE CATEGORY
    // ============================================================

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult>
        Deactivate(
            Guid id)
    {
        try
        {
            var success =
                await _categoryService
                    .DeactivateCategoryAsync(
                        id);

            if (!success)
            {
                return NotFound(new
                {
                    message =
                        "Category not found or already inactive."
                });
            }

            return Ok(new
            {
                message =
                    "Category deactivated successfully."
            });
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
}

// ============================================================
// CREATE CATEGORY REQUEST
// ============================================================

public class CreateCategoryRequest
{
    public string Name { get; set; } =
        string.Empty;

    public string? Description { get; set; }
}

// ============================================================
// UPDATE CATEGORY REQUEST
// ============================================================

public class UpdateCategoryRequest
{
    public string Name { get; set; } =
        string.Empty;

    public string? Description { get; set; }
}