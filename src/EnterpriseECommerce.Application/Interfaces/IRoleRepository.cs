using EnterpriseECommerce.Domain.Entities;

namespace EnterpriseECommerce.Application.Interfaces;

/// <summary>
/// Provides data-access operations for application roles.
///
/// The Application layer depends on this abstraction instead of
/// directly depending on Entity Framework Core.
/// </summary>
public interface IRoleRepository
{
    /// <summary>
    /// Finds a role by its unique name.
    /// </summary>
    Task<Role?> GetByNameAsync(string name);
}