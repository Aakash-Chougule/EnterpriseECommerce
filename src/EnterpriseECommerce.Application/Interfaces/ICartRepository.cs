using EnterpriseECommerce.Domain.Entities;

namespace EnterpriseECommerce.Application.Interfaces;

public interface ICartRepository
{
    Task<Cart?> GetByUserIdAsync(Guid userId);

    Task<Cart?> GetByIdAsync(Guid id);

    Task AddAsync(Cart cart);

    Task UpdateAsync(Cart cart);
}