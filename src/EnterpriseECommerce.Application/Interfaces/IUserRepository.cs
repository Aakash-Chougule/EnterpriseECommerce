using EnterpriseECommerce.Domain.Entities;

namespace EnterpriseECommerce.Application.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(
        string email);

    Task<User?> GetByIdAsync(
        Guid id);

    Task<IReadOnlyList<User>>
        GetAllAsync();

    Task<bool> ExistsByEmailAsync(
        string email);

    Task<bool> ExistsByEmailExceptUserAsync(
        string email,
        Guid userId);

    Task AddAsync(
        User user);

    Task UpdateAsync(
        User user);
}