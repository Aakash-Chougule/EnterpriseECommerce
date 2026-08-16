using EnterpriseECommerce.Domain.Entities;

namespace EnterpriseECommerce.Application.Interfaces;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(Guid id);

    Task<IEnumerable<Order>> GetByUserIdAsync(Guid userId);

    Task AddAsync(Order order);

    Task UpdateAsync(Order order);
}