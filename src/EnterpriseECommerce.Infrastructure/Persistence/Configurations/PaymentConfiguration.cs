using EnterpriseECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnterpriseECommerce.Infrastructure.Persistence.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments");

        builder.HasKey(payment => payment.Id);

        builder.Property(payment => payment.Id)
            .ValueGeneratedNever();

        builder.Property(payment => payment.OrderId)
            .IsRequired();

        builder.Property(payment => payment.Amount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(payment => payment.PaymentMethod)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(payment => payment.TransactionId)
            .HasMaxLength(150);

        builder.Property(payment => payment.Status)
            .IsRequired();

        builder.Property(payment => payment.FailureReason)
            .HasMaxLength(500);

        builder.Property(payment => payment.CreatedAt)
            .IsRequired();

        builder.HasIndex(payment => payment.OrderId);

        builder.HasIndex(payment => payment.TransactionId)
            .IsUnique();

        builder.HasOne<Order>()
            .WithMany()
            .HasForeignKey(payment => payment.OrderId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}