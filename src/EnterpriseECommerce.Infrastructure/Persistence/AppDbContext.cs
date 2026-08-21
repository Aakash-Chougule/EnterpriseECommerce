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

    public DbSet<User> Users => Set<User>();

    public DbSet<Role> Roles => Set<Role>();

    public DbSet<Permission> Permissions =>
        Set<Permission>();

    public DbSet<UserPermission> UserPermissions =>
        Set<UserPermission>();

    public DbSet<Category> Categories => Set<Category>();

    public DbSet<Product> Products => Set<Product>();

    public DbSet<Order> Orders => Set<Order>();

    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    public DbSet<Payment> Payments => Set<Payment>();

    public DbSet<Cart> Carts => Set<Cart>();

    public DbSet<CartItem> CartItems => Set<CartItem>();
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