using EnterpriseECommerce.Application.Interfaces;
using EnterpriseECommerce.Domain.Entities;
using EnterpriseECommerce.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

namespace EnterpriseECommerce.Infrastructure.Repositories;

public class OrderRepository :
    IOrderRepository
{
    private readonly AppDbContext
        _context;

    public OrderRepository(
        AppDbContext context)
    {
        _context =
            context;
    }

    // ========================================================
    // GET BY ID
    // ========================================================

    public async Task<Order?>
        GetByIdAsync(
            Guid id)
    {
        return await _context
            .Orders
            .Include(
                order =>
                    order.OrderItems)
            .FirstOrDefaultAsync(
                order =>
                    order.Id == id);
    }

    // ========================================================
    // GET USER ORDERS
    // ========================================================

    public async Task<IEnumerable<Order>>
        GetByUserIdAsync(
            Guid userId)
    {
        return await _context
            .Orders
            .Include(
                order =>
                    order.OrderItems)
            .Where(
                order =>
                    order.UserId ==
                    userId)
            .OrderByDescending(
                order =>
                    order.CreatedAt)
            .ToListAsync();
    }

    // ========================================================
    // ADMIN / REPORTING - GET ALL ORDERS
    // ========================================================

    public async Task<IEnumerable<Order>>
        GetAllAsync()
    {
        return await _context
            .Orders
            .Include(
                order =>
                    order.OrderItems)
            .OrderByDescending(
                order =>
                    order.CreatedAt)
            .ToListAsync();
    }

    // ========================================================
    // ADD
    // ========================================================

    public async Task AddAsync(
        Order order)
    {
        await _context
            .Orders
            .AddAsync(
                order);

        await _context
            .SaveChangesAsync();
    }

    // ========================================================
    // UPDATE
    // ========================================================

    public async Task UpdateAsync(
        Order order)
    {
        await _context
            .SaveChangesAsync();
    }
}