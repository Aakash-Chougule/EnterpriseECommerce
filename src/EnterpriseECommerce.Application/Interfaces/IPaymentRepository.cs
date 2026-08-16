using EnterpriseECommerce.Domain.Entities;

namespace EnterpriseECommerce.Application.Interfaces;

public interface IPaymentRepository
{
    Task<Payment?> GetByIdAsync(Guid id);

    Task<Payment?> GetByOrderIdAsync(Guid orderId);

    Task AddAsync(Payment payment);

    Task UpdateAsync(Payment payment);
}