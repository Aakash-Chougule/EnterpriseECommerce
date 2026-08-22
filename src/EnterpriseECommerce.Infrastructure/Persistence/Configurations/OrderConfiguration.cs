using EnterpriseECommerce.Domain.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnterpriseECommerce.Infrastructure.Persistence.Configurations;

public class OrderConfiguration :
    IEntityTypeConfiguration<Order>
{
    public void Configure(
        EntityTypeBuilder<Order> builder)
    {
        builder.ToTable(
            "Orders");

        builder.HasKey(
            order =>
                order.Id);

        builder.Property(
                order =>
                    order.OrderNumber)
            .IsRequired()
            .HasMaxLength(
                50);

        // ====================================================
        // FINANCIAL
        // ====================================================

        builder.Property(
                order =>
                    order.Subtotal)
            .HasPrecision(
                18,
                2)
            .HasDefaultValue(
                0m)
            .IsRequired();

        builder.Property(
                order =>
                    order.TaxableAmount)
            .HasPrecision(
                18,
                2)
            .HasDefaultValue(
                0m)
            .IsRequired();

        builder.Property(
                order =>
                    order.TotalGst)
            .HasPrecision(
                18,
                2)
            .HasDefaultValue(
                0m)
            .IsRequired();

        builder.Property(
                order =>
                    order.TotalCgst)
            .HasPrecision(
                18,
                2)
            .HasDefaultValue(
                0m)
            .IsRequired();

        builder.Property(
                order =>
                    order.TotalSgst)
            .HasPrecision(
                18,
                2)
            .HasDefaultValue(
                0m)
            .IsRequired();

        builder.Property(
                order =>
                    order.TotalIgst)
            .HasPrecision(
                18,
                2)
            .HasDefaultValue(
                0m)
            .IsRequired();

        builder.Property(
                order =>
                    order.ShippingCharge)
            .HasPrecision(
                18,
                2)
            .HasDefaultValue(
                0m)
            .IsRequired();

        builder.Property(
                order =>
                    order.DiscountAmount)
            .HasPrecision(
                18,
                2)
            .HasDefaultValue(
                0m)
            .IsRequired();

        builder.Property(
                order =>
                    order.TotalAmount)
            .HasPrecision(
                18,
                2)
            .IsRequired();

        // ====================================================
        // SHIPPING
        // ====================================================

        builder.Property(
                order =>
                    order.ShippingAddress)
            .IsRequired()
            .HasMaxLength(
                500);

        builder.Property(
                order =>
                    order.ShippingState)
            .HasMaxLength(
                100)
            .HasDefaultValue(
                string.Empty)
            .IsRequired();

        builder.Property(
                order =>
                    order.ShippingStateCode)
            .HasMaxLength(
                10)
            .HasDefaultValue(
                string.Empty)
            .IsRequired();

        builder.Property(
                order =>
                    order.PostalCode)
            .HasMaxLength(
                20)
            .HasDefaultValue(
                string.Empty)
            .IsRequired();

        builder.Property(
                order =>
                    order.IsInterState)
            .HasDefaultValue(
                false)
            .IsRequired();

        // ====================================================
        // STATUS
        // ====================================================

        builder.Property(
                order =>
                    order.Status)
            .IsRequired();

        builder.Property(
                order =>
                    order.PaymentStatus)
            .IsRequired();

        builder.Property(
                order =>
                    order.CreatedAt)
            .IsRequired();

        builder.HasIndex(
                order =>
                    order.OrderNumber)
            .IsUnique();

        builder.HasMany(
                order =>
                    order.OrderItems)
            .WithOne()
            .HasForeignKey(
                item =>
                    item.OrderId)
            .OnDelete(
                DeleteBehavior.Cascade);
    }
}