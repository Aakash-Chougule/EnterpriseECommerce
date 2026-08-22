using EnterpriseECommerce.Domain.Entities;

namespace EnterpriseECommerce.Application.Interfaces;

public interface IPaymentRepository
{
    Task<Payment?> GetByIdAsync(
        Guid id);

    Task<Payment?> GetByOrderIdAsync(
        Guid orderId);

    // ========================================================
    // ADMIN / REPORTING
    // ========================================================

    Task<IEnumerable<Payment>>
        GetAllAsync();

    // ========================================================
    // COMMANDS
    // ========================================================

    Task AddAsync(
        Payment payment);

    Task UpdateAsync(
        Payment payment);
}