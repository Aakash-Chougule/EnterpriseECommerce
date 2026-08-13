using EnterpriseECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnterpriseECommerce.Infrastructure.Persistence.Configurations;

/// <summary>
/// Defines the database mapping and relationships for orders.
/// </summary>
public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");

        builder.HasKey(order => order.Id);

        builder.Property(order => order.OrderNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(order => order.TotalAmount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(order => order.ShippingAddress)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(order => order.Status)
            .IsRequired();

        builder.Property(order => order.PaymentStatus)
            .IsRequired();

        builder.Property(order => order.CreatedAt)
            .IsRequired();

        // Each order should have a unique business-friendly order number.
        builder.HasIndex(order => order.OrderNumber)
            .IsUnique();

        // Configure the relationship between Order and OrderItem.
        builder.HasMany(order => order.OrderItems)
            .WithOne()
            .HasForeignKey(item => item.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}