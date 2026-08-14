using EnterpriseECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseECommerce.Infrastructure.Persistence.Seed;

/// <summary>
/// Seeds the default application roles required by the system.
///
/// These roles are created automatically when the application
/// initializes the database.
/// </summary>
public static class RoleSeeder
{
    /// <summary>
    /// Creates the default roles if they do not already exist.
    /// </summary>
    public static async Task SeedAsync(AppDbContext context)
    {
        // Check whether roles have already been created.
        if (await context.Roles.AnyAsync())
        {
            return;
        }

        // Create the roles used by the application.
        var roles = new List<Role>
        {
            new Role(
                "Admin",
                "Full access to the e-commerce platform."),

            new Role(
                "Manager",
                "Can manage products and orders."),

            new Role(
                "Customer",
                "Can browse products and place orders.")
        };

        await context.Roles.AddRangeAsync(roles);

        await context.SaveChangesAsync();
    }
}