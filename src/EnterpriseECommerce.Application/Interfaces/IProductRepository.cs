using EnterpriseECommerce.Domain.Entities;

namespace EnterpriseECommerce.Application.Interfaces;

/// <summary>
/// Defines the operations required to access product data.
///
/// The Application layer depends on this abstraction rather than
/// directly depending on Entity Framework Core.
/// </summary>
public interface IProductRepository
{
    Task<Product?> GetByIdAsync(Guid id);

    Task<IReadOnlyList<Product>> GetAllAsync();

    Task AddAsync(Product product);

    Task UpdateAsync(Product product);

    Task DeleteAsync(Product product);
}