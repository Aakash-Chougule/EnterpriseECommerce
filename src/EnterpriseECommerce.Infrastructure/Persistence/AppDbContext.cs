using EnterpriseECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseECommerce.Infrastructure.Persistence;

/// <summary>
/// Entity Framework Core database context for the e-commerce platform.
///
/// This class represents the application's session with the PostgreSQL database.
/// It exposes DbSet properties for the domain entities that are persisted.
/// </summary>
public class AppDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the AppDbContext.
    /// </summary>
    /// <param name="options">
    /// Database configuration supplied through dependency injection.
    /// </param>
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    // Represents the Users table in PostgreSQL.
    public DbSet<User> Users => Set<User>();

    // Represents the Roles table in PostgreSQL.
    public DbSet<Role> Roles => Set<Role>();

    // Represents the Categories table in PostgreSQL.
    public DbSet<Category> Categories => Set<Category>();

    // Represents the Products table in PostgreSQL.
    public DbSet<Product> Products => Set<Product>();

    // Represents the Orders table in PostgreSQL.
    public DbSet<Order> Orders => Set<Order>();

    // Represents the OrderItems table in PostgreSQL.
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    /// <summary>
    /// Applies entity configurations when the DbContext is created.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Automatically loads all IEntityTypeConfiguration implementations
        // from the Infrastructure assembly.
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(AppDbContext).Assembly);
    }
}