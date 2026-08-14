using EnterpriseECommerce.Application.Interfaces;
using EnterpriseECommerce.Domain.Entities;
using EnterpriseECommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseECommerce.Infrastructure.Repositories;

/// <summary>
/// Entity Framework Core implementation of IProductRepository.
///
/// This class is responsible for communicating with PostgreSQL
/// through AppDbContext.
/// </summary>
public class ProductRepository : IProductRepository
{
    private readonly AppDbContext _context;

    public ProductRepository(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Retrieves a product by its unique identifier.
    /// </summary>
    public async Task<Product?> GetByIdAsync(Guid id)
    {
        return await _context.Products
            .FirstOrDefaultAsync(product => product.Id == id);
    }

    /// <summary>
    /// Retrieves all products.
    ///
    /// AsNoTracking is used because this is a read-only operation.
    /// </summary>
    public async Task<IReadOnlyList<Product>> GetAllAsync()
    {
        return await _context.Products
            .AsNoTracking()
            .ToListAsync();
    }

    /// <summary>
    /// Adds a new product to the database.
    /// </summary>
    public async Task AddAsync(Product product)
    {
        await _context.Products.AddAsync(product);

        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Updates an existing product.
    /// </summary>
    public async Task UpdateAsync(Product product)
    {
        _context.Products.Update(product);

        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Removes a product from the database.
    /// </summary>
    public async Task DeleteAsync(Product product)
    {
        _context.Products.Remove(product);

        await _context.SaveChangesAsync();
    }
}