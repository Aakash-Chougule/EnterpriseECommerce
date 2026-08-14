using EnterpriseECommerce.Domain.Entities;
using EnterpriseECommerce.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseECommerce.Infrastructure.Persistence.Seed;

/// <summary>
/// Seeds the initial administrator account.
///
/// This seeder is idempotent, meaning it can safely execute every
/// time the application starts without creating duplicate users.
///
/// The administrator is created only when the configured admin
/// account does not already exist.
/// </summary>
public static class AdminUserSeeder
{
    /// <summary>
    /// Creates the initial administrator account if it does not exist.
    /// </summary>
    /// <param name="context">
    /// Application database context used to access PostgreSQL.
    /// </param>
    public static async Task SeedAsync(AppDbContext context)
    {
        // ------------------------------------------------------------
        // Find the Admin role.
        // ------------------------------------------------------------
        // The Admin role must be created by RoleSeeder before this
        // seeder runs.
        // ------------------------------------------------------------

        var adminRole = await context.Roles
            .FirstOrDefaultAsync(role => role.Name == "Admin");

        if (adminRole is null)
        {
            throw new InvalidOperationException(
                "Admin role must exist before the Admin user can be seeded.");
        }

        // ------------------------------------------------------------
        // Define the initial administrator account.
        // ------------------------------------------------------------
        // This account is used only for local/development setup.
        //
        // In production, these values should come from secure
        // environment variables or a secret-management service.
        // ------------------------------------------------------------

        const string adminEmail =
            "admin@enterpriseecommerce.com";

        const string adminPassword =
            "Admin@12345";

        // ------------------------------------------------------------
        // Check whether the administrator already exists.
        // ------------------------------------------------------------
        // Because the Email column is unique, we use the email to
        // identify the initial administrator.
        //
        // If the user already exists, we do nothing.
        // ------------------------------------------------------------

        var existingAdmin = await context.Users
            .FirstOrDefaultAsync(user => user.Email == adminEmail);

        if (existingAdmin is not null)
        {
            return;
        }

        // ------------------------------------------------------------
        // Hash the administrator password.
        // ------------------------------------------------------------
        // NEVER store a plain-text password in the database.
        //
        // BCrypt automatically generates a salt and securely hashes
        // the supplied password.
        // ------------------------------------------------------------

        var passwordHasher = new BCryptPasswordHasher();

        var passwordHash = passwordHasher.Hash(adminPassword);

        // ------------------------------------------------------------
        // Create the administrator user.
        // ------------------------------------------------------------

        var adminUser = new User(
            firstName: "System",
            lastName: "Administrator",
            email: adminEmail,
            passwordHash: passwordHash,
            roleId: adminRole.Id);

        // ------------------------------------------------------------
        // Add the administrator to the database.
        // ------------------------------------------------------------

        context.Users.Add(adminUser);

        await context.SaveChangesAsync();
    }
}