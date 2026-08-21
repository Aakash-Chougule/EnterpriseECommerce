using EnterpriseECommerce.Domain.Entities;

using Microsoft.EntityFrameworkCore;

namespace EnterpriseECommerce.Infrastructure.Persistence.Seed;

public static class RoleSeeder
{
    public static async Task SeedAsync(
        AppDbContext context)
    {
        await EnsureRoleAsync(
            context,
            "Admin",
            "Administrative user with individually assigned permissions.");

        await EnsureRoleAsync(
            context,
            "Manager",
            "Manager account.");

        await EnsureRoleAsync(
            context,
            "Customer",
            "Customer account.");

        await context.SaveChangesAsync();
    }

    private static async Task EnsureRoleAsync(
        AppDbContext context,
        string name,
        string description)
    {
        var exists =
            await context.Roles
                .AnyAsync(
                    role =>
                        role.Name ==
                        name);

        if (exists)
        {
            return;
        }

        context.Roles.Add(
            new Role(
                name,
                description));
    }
}