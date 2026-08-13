using EnterpriseECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnterpriseECommerce.Infrastructure.Persistence.Configurations;

/// <summary>
/// Defines the database mapping and constraints for products.
/// </summary>
public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");

        builder.HasKey(product => product.Id);

        builder.Property(product => product.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(product => product.Description)
            .HasMaxLength(2000);

        builder.Property(product => product.SKU)
            .IsRequired()
            .HasMaxLength(50);

        // Explicit precision prevents floating-point issues
        // when storing monetary values.
        builder.Property(product => product.Price)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(product => product.StockQuantity)
            .IsRequired();

        builder.Property(product => product.IsActive)
            .IsRequired();

        builder.Property(product => product.CreatedAt)
            .IsRequired();

        // SKU should uniquely identify a product.
        builder.HasIndex(product => product.SKU)
            .IsUnique();

        // A Product belongs to one Category.
        // A Category can contain many Products.
        builder.HasOne<Category>()
            .WithMany()
            .HasForeignKey(product => product.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}