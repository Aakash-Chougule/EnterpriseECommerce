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

    public CategoryService(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    /// <summary>
    /// Retrieves all active categories.
    /// </summary>
    public async Task<IReadOnlyList<Category>> GetAllCategoriesAsync()
    {
        var categories = await _categoryRepository.GetAllAsync();

        return categories
            .Where(category => category.IsActive)
            .ToList();
    }

    /// <summary>
    /// Retrieves an active category by ID.
    /// </summary>
    public async Task<Category?> GetCategoryByIdAsync(Guid id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);

        if (category is null || !category.IsActive)
        {
            return null;
        }

        return category;
    }

    /// <summary>
    /// Creates a new category.
    /// </summary>
    public async Task<Category> CreateCategoryAsync(
        string name,
        string? description)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "Category name is required.");
        }

        var category = new Category(
            name.Trim(),
            description?.Trim());

        await _categoryRepository.AddAsync(category);

        return category;
    }
}