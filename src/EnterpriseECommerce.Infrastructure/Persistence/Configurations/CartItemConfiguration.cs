using EnterpriseECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnterpriseECommerce.Infrastructure.Persistence.Configurations;

/// <summary>
/// Defines the database mapping for cart items.
/// </summary>
public class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
{
    public void Configure(EntityTypeBuilder<CartItem> builder)
    {
        builder.ToTable("CartItems");

        // ------------------------------------------------------------
        // Primary key
        //
        // CartItem generates its own Guid inside the domain entity.
        // Therefore EF Core must NOT treat this key as database
        // generated.
        // ------------------------------------------------------------

        builder.HasKey(item => item.Id);

        builder.Property(item => item.Id)
            .ValueGeneratedNever();

        // ------------------------------------------------------------
        // CartId
        // ------------------------------------------------------------

        builder.Property(item => item.CartId)
            .IsRequired();

        // ------------------------------------------------------------
        // ProductId
        // ------------------------------------------------------------

        builder.Property(item => item.ProductId)
            .IsRequired();

        // ------------------------------------------------------------
        // Quantity
        // ------------------------------------------------------------

        builder.Property(item => item.Quantity)
            .IsRequired();

        // ------------------------------------------------------------
        // A product can appear only once inside a cart.
        //
        // If the same product is added again, Cart.AddItem()
        // increases its quantity instead of creating another row.
        // ------------------------------------------------------------

        builder.HasIndex(item => new
        {
            item.CartId,
            item.ProductId
        })
        .IsUnique();

        // ------------------------------------------------------------
        // CartItem → Product
        // ------------------------------------------------------------

        builder.HasOne<Product>()
            .WithMany()
            .HasForeignKey(item => item.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
