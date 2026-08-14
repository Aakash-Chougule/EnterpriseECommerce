using EnterpriseECommerce.Application.Interfaces;
using EnterpriseECommerce.Domain.Entities;
using EnterpriseECommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseECommerce.Infrastructure.Repositories;

/// <summary>
/// Entity Framework Core implementation of IUserRepository.
///
/// This class contains the actual database-access logic for users.
/// The Application layer only knows about IUserRepository and does
/// not depend directly on Entity Framework Core.
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Retrieves a user by email address.
    ///
    /// The Role navigation property is included because the
    /// authentication process needs the user's role when
    /// generating the JWT token.
    /// </summary>
    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users
            .Include(user => user.Role)
            .FirstOrDefaultAsync(user => user.Email == email);
    }

    /// <summary>
    /// Retrieves a user by unique identifier.
    /// </summary>
    public async Task<User?> GetByIdAsync(Guid id)
    {
        return await _context.Users
            .Include(user => user.Role)
            .FirstOrDefaultAsync(user => user.Id == id);
    }

    /// <summary>
    /// Adds a new user to the database.
    /// </summary>
    public async Task AddAsync(User user)
    {
        await _context.Users.AddAsync(user);

        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Determines whether the supplied email address is already
    /// registered in the system.
    /// </summary>
    public async Task<bool> ExistsByEmailAsync(string email)
    {
        return await _context.Users
            .AnyAsync(user => user.Email == email);
    }
}