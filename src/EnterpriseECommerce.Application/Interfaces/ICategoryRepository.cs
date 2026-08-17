using EnterpriseECommerce.Domain.Entities;

namespace EnterpriseECommerce.Application.Interfaces;

/// <summary>
/// Defines the operations required to access category data.
///
/// The Application layer depends on this abstraction rather than
/// directly depending on Entity Framework Core or PostgreSQL.
/// </summary>
public interface ICategoryRepository
{
    /// <summary>
    /// Retrieves a category by its unique identifier.
    /// </summary>
    Task<Category?> GetByIdAsync(Guid id);

    Task<Category?> GetByNameAsync(string name);

    /// <summary>
    /// Retrieves all categories.
    /// </summary>
    Task<IReadOnlyList<Category>> GetAllAsync();

    /// <summary>
    /// Adds a new category.
    /// </summary>
    Task AddAsync(Category category);

    /// <summary>
    /// Updates an existing category.
    /// </summary>
    Task UpdateAsync(Category category);

    /// <summary>
    /// Removes a category.
    /// </summary>
    Task DeleteAsync(Category category);
}