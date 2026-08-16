using EnterpriseECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnterpriseECommerce.Infrastructure.Persistence.Configurations;

/// <summary>
/// Defines the database mapping for shopping carts.
/// </summary>
public class CartConfiguration : IEntityTypeConfiguration<Cart>
{
    public void Configure(EntityTypeBuilder<Cart> builder)
    {
        builder.ToTable("Carts");

        // ------------------------------------------------------------
        // Primary key
        //
        // Cart generates its own Guid inside the domain entity.
        // Therefore EF Core must not generate the key.
        // ------------------------------------------------------------

        builder.HasKey(cart => cart.Id);

        builder.Property(cart => cart.Id)
            .ValueGeneratedNever();

        // ------------------------------------------------------------
        // UserId
        // ------------------------------------------------------------

        builder.Property(cart => cart.UserId)
            .IsRequired();

        // ------------------------------------------------------------
        // CreatedAt
        // ------------------------------------------------------------

        builder.Property(cart => cart.CreatedAt)
            .IsRequired();

        // ------------------------------------------------------------
        // One user can have only one active/current cart.
        // ------------------------------------------------------------

        builder.HasIndex(cart => cart.UserId)
            .IsUnique();

        // ------------------------------------------------------------
        // Cart → CartItems
        // ------------------------------------------------------------

        builder.HasMany(cart => cart.Items)
            .WithOne()
            .HasForeignKey(item => item.CartId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
