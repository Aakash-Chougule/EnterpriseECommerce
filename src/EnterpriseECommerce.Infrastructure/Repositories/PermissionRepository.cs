using EnterpriseECommerce.Application.Interfaces;
using EnterpriseECommerce.Domain.Entities;
using EnterpriseECommerce.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

namespace EnterpriseECommerce.Infrastructure.Repositories;

public class PermissionRepository :
    IPermissionRepository
{
    private readonly AppDbContext _context;

    public PermissionRepository(
        AppDbContext context)
    {
        _context =
            context;
    }

    public async Task<IReadOnlyList<Permission>>
        GetAllAsync()
    {
        return await _context.Permissions
            .OrderBy(
                permission =>
                    permission.Name)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Permission>>
        GetByNamesAsync(
            IEnumerable<string> names)
    {
        var requested =
            names
                .Distinct(
                    StringComparer.OrdinalIgnoreCase)
                .ToList();

        return await _context.Permissions
            .Where(
                permission =>
                    requested.Contains(
                        permission.Name))
            .ToListAsync();
    }
}