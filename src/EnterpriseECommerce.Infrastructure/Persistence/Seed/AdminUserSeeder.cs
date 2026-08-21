using EnterpriseECommerce.Domain.Entities;
using EnterpriseECommerce.Infrastructure.Security;

using Microsoft.EntityFrameworkCore;

namespace EnterpriseECommerce.Infrastructure.Persistence.Seed;

public static class AdminUserSeeder
{
    public static async Task SeedAsync(
        AppDbContext context)
    {
        var adminRole =
            await context.Roles
                .FirstOrDefaultAsync(
                    role =>
                        role.Name ==
                        "Admin")
            ?? throw new InvalidOperationException(
                "Admin role must exist.");

        const string adminEmail =
            "admin@enterpriseecommerce.com";

        const string adminPassword =
            "Admin@12345";

        var adminUser =
            await context.Users
                .Include(
                    user =>
                        user.UserPermissions)
                .ThenInclude(
                    item =>
                        item.Permission)
                .FirstOrDefaultAsync(
                    user =>
                        user.Email ==
                        adminEmail);

        if (adminUser is null)
        {
            var passwordHasher =
                new BCryptPasswordHasher();

            adminUser =
                new User(
                    "System",
                    "Administrator",
                    adminEmail,
                    passwordHasher.Hash(
                        adminPassword),
                    adminRole.Id);

            context.Users.Add(
                adminUser);

            await context.SaveChangesAsync();
        }
        else
        {
            adminUser.AssignRole(
                adminRole);
        }

        // ========================================================
        // PERMANENT MAIN ADMIN
        // ========================================================

        adminUser.MarkAsMainAdmin();

        adminUser.Activate();

        // ========================================================
        // GIVE MAIN ADMIN ALL PERMISSIONS
        // ========================================================

        var allPermissions =
            await context.Permissions
                .ToListAsync();

        foreach (var permission in
                 allPermissions)
        {
            adminUser.AddPermission(
                permission);
        }

        await context.SaveChangesAsync();
    }
}