using EnterpriseECommerce.Application.Security;
using EnterpriseECommerce.Domain.Entities;

using Microsoft.EntityFrameworkCore;

namespace EnterpriseECommerce.Infrastructure.Persistence.Seed;

public static class PermissionSeeder
{
    public static async Task SeedAsync(
        AppDbContext context)
    {
        // Get permission names that already exist.
        var existingNames =
            await context.Permissions
                .AsNoTracking()
                .Select(permission =>
                    permission.Name)
                .ToListAsync();

        var existingSet =
            existingNames.ToHashSet(
                StringComparer.OrdinalIgnoreCase);

        // Only create missing permissions.
        var missingPermissions =
            PermissionNames.All
                .Where(name =>
                    !existingSet.Contains(name))
                .Select(name =>
                    new Permission(
                        name,
                        GetDescription(name)))
                .ToList();

        if (missingPermissions.Count == 0)
        {
            return;
        }

        await context.Permissions
            .AddRangeAsync(
                missingPermissions);

        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            // Another integration-test host may have inserted
            // the same permissions between our SELECT and INSERT.
            //
            // Clear the failed tracked entities and verify that
            // all required permissions now exist.

            context.ChangeTracker.Clear();

            var databaseNames =
                await context.Permissions
                    .AsNoTracking()
                    .Select(permission =>
                        permission.Name)
                    .ToListAsync();

            var databaseSet =
                databaseNames.ToHashSet(
                    StringComparer.OrdinalIgnoreCase);

            var stillMissing =
                PermissionNames.All
                    .Where(name =>
                        !databaseSet.Contains(name))
                    .ToList();

            if (stillMissing.Count > 0)
            {
                throw;
            }
        }
    }

    private static string GetDescription(
        string permission)
    {
        return permission switch
        {
            PermissionNames.ManageProducts =>
                "Create, update, activate and deactivate products.",

            PermissionNames.ManageCategories =>
                "Create, update, activate and deactivate categories.",

            PermissionNames.ManageInventory =>
                "Manage product stock and inventory.",

            PermissionNames.ManageOrders =>
                "View and manage customer orders.",

            PermissionNames.ManagePayments =>
                "View and manage payments.",

            PermissionNames.ManageUsers =>
                "View and manage customer accounts.",

            PermissionNames.ManageAdmins =>
                "Create administrators and manage administrator permissions.",

            PermissionNames.ViewReports =>
                "View administrative reports.",

            _ =>
                permission
        };
    }
}