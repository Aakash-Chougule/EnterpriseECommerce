using EnterpriseECommerce.Application.Services;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseECommerce.API.Controllers;

/// <summary>
/// Provides HTTP endpoints for managing product categories.
///
/// The controller is responsible for handling HTTP requests and
/// delegating business operations to CategoryService.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly CategoryService _categoryService;

    public CategoriesController(CategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    // ------------------------------------------------------------
    // GET: api/Categories
    // ------------------------------------------------------------
    // Returns all active categories.
    //
    // Reading categories is currently public.
    // ------------------------------------------------------------

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var categories =
            await _categoryService.GetAllCategoriesAsync();

        return Ok(categories);
    }

    // ------------------------------------------------------------
    // GET: api/Categories/{id}
    // ------------------------------------------------------------
    // Returns one active category.
    // ------------------------------------------------------------

    [AllowAnonymous]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var category =
            await _categoryService.GetCategoryByIdAsync(id);

        if (category is null)
        {
            return NotFound(new
            {
                message = "Category not found."
            });
        }

        return Ok(category);
    }

    // ------------------------------------------------------------
    // POST: api/Categories
    // ------------------------------------------------------------
    // Creates a new category.
    //
    // Only Admin users can create categories.
    // ------------------------------------------------------------

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateCategoryRequest request)
    {
        try
        {
            var category =
                await _categoryService.CreateCategoryAsync(
                    request.Name,
                    request.Description);

            return CreatedAtAction(
                nameof(GetById),
                new { id = category.Id },
                category);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }
}

/// <summary>
/// Request model used when creating a category.
/// </summary>
public class CreateCategoryRequest
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }
}