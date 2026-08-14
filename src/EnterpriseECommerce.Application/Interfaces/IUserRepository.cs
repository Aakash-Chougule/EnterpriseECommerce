using EnterpriseECommerce.Domain.Entities;

namespace EnterpriseECommerce.Application.Interfaces;

/// <summary>
/// Defines data-access operations required for application users.
///
/// The Application layer depends on this abstraction instead of
/// directly depending on Entity Framework Core or PostgreSQL.
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Finds a user by email address.
    /// </summary>
    Task<User?> GetByEmailAsync(string email);

    /// <summary>
    /// Finds a user by unique identifier.
    /// </summary>
    Task<User?> GetByIdAsync(Guid id);

    /// <summary>
    /// Adds a new user to the database.
    /// </summary>
    Task AddAsync(User user);

    /// <summary>
    /// Checks whether an email address is already registered.
    /// </summary>
    Task<bool> ExistsByEmailAsync(string email);
}