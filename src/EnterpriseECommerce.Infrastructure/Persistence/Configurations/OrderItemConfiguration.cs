using EnterpriseECommerce.Domain.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnterpriseECommerce.Infrastructure.Persistence.Configurations;

public class OrderItemConfiguration :
    IEntityTypeConfiguration<OrderItem>
{
    public void Configure(
        EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable(
            "OrderItems");

        builder.HasKey(
            item =>
                item.Id);

        builder.Property(
                item =>
                    item.ProductName)
            .IsRequired()
            .HasMaxLength(
                200);

        builder.Property(
                item =>
                    item.SKU)
            .HasMaxLength(
                50)
            .HasDefaultValue(
                string.Empty);

        builder.Property(
                item =>
                    item.HsnCode)
            .HasMaxLength(
                20)
            .HasDefaultValue(
                string.Empty);

        builder.Property(
                item =>
                    item.Quantity)
            .IsRequired();

        builder.Property(
                item =>
                    item.UnitPrice)
            .HasPrecision(
                18,
                2)
            .IsRequired();

        builder.Property(
                item =>
                    item.GstRate)
            .HasPrecision(
                5,
                2)
            .HasDefaultValue(
                0m)
            .IsRequired();

        builder.Property(
                item =>
                    item.TaxableAmount)
            .HasPrecision(
                18,
                2)
            .HasDefaultValue(
                0m)
            .IsRequired();

        builder.Property(
                item =>
                    item.GstAmount)
            .HasPrecision(
                18,
                2)
            .HasDefaultValue(
                0m)
            .IsRequired();

        builder.Property(
                item =>
                    item.CgstAmount)
            .HasPrecision(
                18,
                2)
            .HasDefaultValue(
                0m)
            .IsRequired();

        builder.Property(
                item =>
                    item.SgstAmount)
            .HasPrecision(
                18,
                2)
            .HasDefaultValue(
                0m)
            .IsRequired();

        builder.Property(
                item =>
                    item.IgstAmount)
            .HasPrecision(
                18,
                2)
            .HasDefaultValue(
                0m)
            .IsRequired();

        builder.Property(
                item =>
                    item.TotalPrice)
            .HasPrecision(
                18,
                2)
            .HasDefaultValue(
                0m)
            .IsRequired();
    }
}