using EnterpriseECommerce.Application.Interfaces;
using EnterpriseECommerce.Domain.Entities;
using EnterpriseECommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseECommerce.Infrastructure.Repositories;

/// <summary>
/// Entity Framework Core implementation of IRoleRepository.
///
/// Responsible for retrieving role information from PostgreSQL.
/// </summary>
public class RoleRepository : IRoleRepository
{
    private readonly AppDbContext _context;

    public RoleRepository(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Retrieves a role using its unique name.
    /// </summary>
    public async Task<Role?> GetByNameAsync(string name)
    {
        return await _context.Roles
            .FirstOrDefaultAsync(role => role.Name == name);
    }
}