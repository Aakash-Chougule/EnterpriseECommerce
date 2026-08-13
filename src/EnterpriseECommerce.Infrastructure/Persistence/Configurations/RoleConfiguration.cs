using EnterpriseECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnterpriseECommerce.Infrastructure.Persistence.Configurations;

/// <summary>
/// Defines the database mapping for application roles.
/// </summary>
public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("Roles");

        builder.HasKey(role => role.Id);

        builder.Property(role => role.Name)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(role => role.Description)
            .HasMaxLength(250);

        // Role names must be unique.
        builder.HasIndex(role => role.Name)
            .IsUnique();
    }
}