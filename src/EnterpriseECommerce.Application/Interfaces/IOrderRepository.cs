using EnterpriseECommerce.Domain.Entities;

namespace EnterpriseECommerce.Application.Interfaces;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(Guid id);

    Task<IEnumerable<Order>> GetByUserIdAsync(Guid userId);

    // Admin: retrieve all orders.
    Task<IEnumerable<Order>> GetAllAsync();

    Task AddAsync(Order order);

    Task UpdateAsync(Order order);
}