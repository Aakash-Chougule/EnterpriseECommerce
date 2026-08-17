using EnterpriseECommerce.Application.Interfaces;
using EnterpriseECommerce.Domain.Entities;
using EnterpriseECommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseECommerce.Infrastructure.Repositories;

/// <summary>
/// Entity Framework Core implementation of ICategoryRepository.
///
/// This class is responsible for communicating with PostgreSQL
/// through AppDbContext.
/// </summary>
public class CategoryRepository : ICategoryRepository
{
    private readonly AppDbContext _context;

    public CategoryRepository(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Retrieves a category by its unique identifier.
    /// </summary>
    public async Task<Category?> GetByIdAsync(Guid id)
    {
        return await _context.Categories
            .FirstOrDefaultAsync(category => category.Id == id);
    }

    /// <summary>
    /// Retrieves all categories.
    ///
    /// AsNoTracking is used because this is a read-only operation.
    /// </summary>
    public async Task<IReadOnlyList<Category>> GetAllAsync()
    {
        return await _context.Categories
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Category?> GetByNameAsync(
    string name)
    {
        return await _context.Categories
            .FirstOrDefaultAsync(
                category =>
                    category.Name.ToLower() ==
                    name.ToLower());
    }

    /// <summary>
    /// Adds a new category to the database.
    /// </summary>
    public async Task AddAsync(Category category)
    {
        await _context.Categories.AddAsync(category);

        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Updates an existing category.
    /// </summary>
    public async Task UpdateAsync(Category category)
    {
        _context.Categories.Update(category);

        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Removes a category from the database.
    /// </summary>
    public async Task DeleteAsync(Category category)
    {
        _context.Categories.Remove(category);

        await _context.SaveChangesAsync();
    }
}