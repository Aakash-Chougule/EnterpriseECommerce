using EnterpriseECommerce.Domain.Entities;

namespace EnterpriseECommerce.Infrastructure.Persistence;

/// <summary>
/// Provides initial development data for the application.
///
/// The seeder checks whether category data already exists before
/// inserting records, preventing duplicate seed data on every
/// application startup.
/// </summary>
public static class DbSeeder
{
    /// <summary>
    /// Inserts initial product and category data into the database.
    /// </summary>
    public static async Task SeedAsync(AppDbContext context)
    {
        // --------------------------------------------------------
        // Prevent duplicate seed data.
        //
        // If at least one category exists, we assume the database
        // has already been seeded.
        // --------------------------------------------------------
        if (context.Categories.Any())
        {
            return;
        }

        // --------------------------------------------------------
        // Create initial category.
        // --------------------------------------------------------
        var electronics = new Category(
            "Electronics",
            "Electronic devices and accessories");

        context.Categories.Add(electronics);

        // --------------------------------------------------------
        // Create sample products.
        //
        // These records allow us to test our API immediately
        // without manually inserting data through pgAdmin.
        // --------------------------------------------------------
        var keyboard = new Product(
            electronics.Id,
            "Mechanical Keyboard",
            "RGB Mechanical Keyboard",
            "KB-1001",
            3499,
            25);

        var mouse = new Product(
            electronics.Id,
            "Gaming Mouse",
            "Wireless Gaming Mouse",
            "MS-1001",
            1999,
            40);

        context.Products.AddRange(keyboard, mouse);

        // --------------------------------------------------------
        // Save all seed data to PostgreSQL.
        // --------------------------------------------------------
        await context.SaveChangesAsync();
    }
}