using EnterpriseECommerce.Domain.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnterpriseECommerce.Infrastructure.Persistence.Configurations;

public class ProductConfiguration :
    IEntityTypeConfiguration<Product>
{
    public void Configure(
        EntityTypeBuilder<Product> builder)
    {
        builder.ToTable(
            "Products");

        builder.HasKey(
            product =>
                product.Id);

        builder.Property(
                product =>
                    product.Name)
            .IsRequired()
            .HasMaxLength(
                200);

        builder.Property(
                product =>
                    product.Description)
            .HasMaxLength(
                2000);

        builder.Property(
                product =>
                    product.SKU)
            .IsRequired()
            .HasMaxLength(
                50);

        // ====================================================
        // GST
        // ====================================================

        builder.Property(
                product =>
                    product.HsnCode)
            .HasMaxLength(
                20)
            .HasDefaultValue(
                string.Empty);

        builder.Property(
                product =>
                    product.GstRate)
            .HasPrecision(
                5,
                2)
            .HasDefaultValue(
                0m)
            .IsRequired();

        // ====================================================
        // MONEY
        // ====================================================

        builder.Property(
                product =>
                    product.Price)
            .HasPrecision(
                18,
                2)
            .IsRequired();

        builder.Property(
                product =>
                    product.StockQuantity)
            .IsRequired();

        builder.Property(
                product =>
                    product.IsActive)
            .IsRequired();

        builder.Property(
                product =>
                    product.CreatedAt)
            .IsRequired();

        builder.HasIndex(
                product =>
                    product.SKU)
            .IsUnique();

        builder.HasOne<Category>()
            .WithMany()
            .HasForeignKey(
                product =>
                    product.CategoryId)
            .OnDelete(
                DeleteBehavior.Restrict);
    }
}