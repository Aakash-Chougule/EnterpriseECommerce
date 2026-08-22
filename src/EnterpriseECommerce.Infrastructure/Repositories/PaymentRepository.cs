using EnterpriseECommerce.Application.Interfaces;
using EnterpriseECommerce.Domain.Entities;
using EnterpriseECommerce.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

namespace EnterpriseECommerce.Infrastructure.Repositories;

public class PaymentRepository :
    IPaymentRepository
{
    private readonly AppDbContext
        _context;

    public PaymentRepository(
        AppDbContext context)
    {
        _context =
            context;
    }

    // ========================================================
    // GET PAYMENT BY ID
    // ========================================================

    public async Task<Payment?>
        GetByIdAsync(
            Guid id)
    {
        return await _context
            .Payments
            .FirstOrDefaultAsync(
                payment =>
                    payment.Id == id);
    }

    // ========================================================
    // GET PAYMENT BY ORDER
    // ========================================================

    public async Task<Payment?>
        GetByOrderIdAsync(
            Guid orderId)
    {
        return await _context
            .Payments
            .FirstOrDefaultAsync(
                payment =>
                    payment.OrderId ==
                    orderId);
    }

    // ========================================================
    // ADMIN / REPORTING
    // ========================================================

    public async Task<IEnumerable<Payment>>
        GetAllAsync()
    {
        return await _context
            .Payments
            .OrderByDescending(
                payment =>
                    payment.CreatedAt)
            .ToListAsync();
    }

    // ========================================================
    // ADD
    // ========================================================

    public async Task AddAsync(
        Payment payment)
    {
        await _context
            .Payments
            .AddAsync(
                payment);

        await _context
            .SaveChangesAsync();
    }

    // ========================================================
    // UPDATE
    // ========================================================

    public async Task UpdateAsync(
        Payment payment)
    {
        await _context
            .SaveChangesAsync();
    }
}