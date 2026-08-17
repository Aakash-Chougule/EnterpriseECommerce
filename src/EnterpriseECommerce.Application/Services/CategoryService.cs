using EnterpriseECommerce.Application.Interfaces;
using EnterpriseECommerce.Domain.Entities;

namespace EnterpriseECommerce.Application.Services;

/// <summary>
/// Contains category-related business logic.
///
/// Controllers delegate category operations to this service
/// instead of directly accessing the repository.
/// </summary>
public class CategoryService
{
    private readonly ICategoryRepository _categoryRepository;

    public CategoryService(
        ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    // ============================================================
    // GET ALL ACTIVE CATEGORIES
    // ============================================================

    /// <summary>
    /// Retrieves all active categories.
    /// </summary>
    public async Task<IReadOnlyList<Category>>
        GetAllCategoriesAsync()
    {
        var categories =
            await _categoryRepository
                .GetAllAsync();

        return categories
            .Where(category =>
                category.IsActive)
            .ToList();
    }

    // ============================================================
    // ADMIN - GET ALL CATEGORIES
    // ============================================================
    //
    // Returns both active and inactive categories.
    // ============================================================

    public async Task<IReadOnlyList<Category>>
        GetAllCategoriesForAdminAsync()
    {
        return await _categoryRepository
            .GetAllAsync();
    }

    // ============================================================
    // GET CATEGORY BY ID
    // ============================================================

    /// <summary>
    /// Retrieves an active category by ID.
    /// </summary>
    public async Task<Category?>
        GetCategoryByIdAsync(
            Guid id)
    {
        var category =
            await _categoryRepository
                .GetByIdAsync(id);

        if (category is null ||
            !category.IsActive)
        {
            return null;
        }

        return category;
    }

    // ============================================================
    // CREATE CATEGORY
    // ============================================================

    /// <summary>
    /// Creates a new category.
    ///
    /// If a category with the same name already exists and is
    /// inactive, the existing category is reactivated instead
    /// of creating a duplicate database record.
    /// </summary>
    public async Task<Category>
        CreateCategoryAsync(
            string name,
            string? description)
    {
        if (string.IsNullOrWhiteSpace(
            name))
        {
            throw new ArgumentException(
                "Category name is required.");
        }

        var normalizedName =
            name.Trim();

        var normalizedDescription =
            description?.Trim();

        // --------------------------------------------------------
        // Search by category name.
        //
        // GetByNameAsync should return both active and inactive
        // categories.
        // --------------------------------------------------------

        var existingCategory =
            await _categoryRepository
                .GetByNameAsync(
                    normalizedName);

        if (existingCategory is not null)
        {
            // ----------------------------------------------------
            // ACTIVE CATEGORY WITH SAME NAME
            // ----------------------------------------------------

            if (existingCategory.IsActive)
            {
                throw new InvalidOperationException(
                    "A category with this name already exists.");
            }

            // ----------------------------------------------------
            // INACTIVE CATEGORY WITH SAME NAME
            // ----------------------------------------------------
            //
            // Restore the old record instead of creating
            // another category with the same name.
            // ----------------------------------------------------

            existingCategory.Update(
                normalizedName,
                normalizedDescription);

            existingCategory.Activate();

            await _categoryRepository
                .UpdateAsync(
                    existingCategory);

            return existingCategory;
        }

        // --------------------------------------------------------
        // Completely new category.
        // --------------------------------------------------------

        var category =
            new Category(
                normalizedName,
                normalizedDescription);

        await _categoryRepository
            .AddAsync(category);

        return category;
    }

    // ============================================================
    // UPDATE CATEGORY
    // ============================================================

    public async Task<Category?>
        UpdateCategoryAsync(
            Guid id,
            string name,
            string? description)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException(
                "CategoryId is required.");
        }

        if (string.IsNullOrWhiteSpace(
            name))
        {
            throw new ArgumentException(
                "Category name is required.");
        }

        var category =
            await _categoryRepository
                .GetByIdAsync(id);

        if (category is null ||
            !category.IsActive)
        {
            return null;
        }

        category.Update(
            name.Trim(),
            description?.Trim());

        await _categoryRepository
            .UpdateAsync(category);

        return category;
    }

    // ============================================================
    // DEACTIVATE CATEGORY
    // ============================================================

    public async Task<bool>
        DeactivateCategoryAsync(
            Guid id)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException(
                "CategoryId is required.");
        }

        var category =
            await _categoryRepository
                .GetByIdAsync(id);

        if (category is null ||
            !category.IsActive)
        {
            return false;
        }

        category.Deactivate();

        await _categoryRepository
            .UpdateAsync(category);

        return true;
    }
}