using EnterpriseECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnterpriseECommerce.Infrastructure.Persistence.Configurations;

/// <summary>
/// Defines the database mapping for product categories.
/// </summary>
public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");

        builder.HasKey(category => category.Id);

        builder.Property(category => category.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(category => category.Description)
            .HasMaxLength(500);

        builder.Property(category => category.IsActive)
            .IsRequired();

        builder.Property(category => category.CreatedAt)
            .IsRequired();

        // Prevent duplicate category names.
        builder.HasIndex(category => category.Name)
            .IsUnique();
    }
}