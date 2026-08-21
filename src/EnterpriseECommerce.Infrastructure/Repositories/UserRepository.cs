using EnterpriseECommerce.Application.Interfaces;
using EnterpriseECommerce.Domain.Entities;
using EnterpriseECommerce.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

namespace EnterpriseECommerce.Infrastructure.Repositories;

public class UserRepository :
    IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(
        AppDbContext context)
    {
        _context =
            context;
    }

    public async Task<User?>
        GetByEmailAsync(
            string email)
    {
        return await _context.Users
            .Include(
                user =>
                    user.Role)
            .Include(
                user =>
                    user.UserPermissions)
            .ThenInclude(
                item =>
                    item.Permission)
            .FirstOrDefaultAsync(
                user =>
                    user.Email ==
                    email);
    }

    public async Task<User?>
        GetByIdAsync(
            Guid id)
    {
        return await _context.Users
            .Include(
                user =>
                    user.Role)
            .Include(
                user =>
                    user.UserPermissions)
            .ThenInclude(
                item =>
                    item.Permission)
            .FirstOrDefaultAsync(
                user =>
                    user.Id ==
                    id);
    }

    public async Task<IReadOnlyList<User>>
        GetAllAsync()
    {
        return await _context.Users
            .Include(
                user =>
                    user.Role)
            .Include(
                user =>
                    user.UserPermissions)
            .ThenInclude(
                item =>
                    item.Permission)
            .OrderByDescending(
                user =>
                    user.IsMainAdmin)
            .ThenBy(
                user =>
                    user.FirstName)
            .ToListAsync();
    }

    public async Task<bool>
        ExistsByEmailAsync(
            string email)
    {
        return await _context.Users
            .AnyAsync(
                user =>
                    user.Email ==
                    email);
    }

    public async Task<bool>
        ExistsByEmailExceptUserAsync(
            string email,
            Guid userId)
    {
        return await _context.Users
            .AnyAsync(
                user =>
                    user.Email ==
                    email &&
                    user.Id !=
                    userId);
    }

    public async Task AddAsync(
        User user)
    {
        await _context.Users
            .AddAsync(
                user);

        await _context
            .SaveChangesAsync();
    }

    public async Task UpdateAsync(
        User user)
    {
        await _context
            .SaveChangesAsync();
    }
}