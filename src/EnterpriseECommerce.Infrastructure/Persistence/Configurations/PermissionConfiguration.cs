using EnterpriseECommerce.Domain.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnterpriseECommerce.Infrastructure.Persistence.Configurations;

public class PermissionConfiguration :
    IEntityTypeConfiguration<Permission>
{
    public void Configure(
        EntityTypeBuilder<Permission> builder)
    {
        builder.ToTable(
            "Permissions");

        builder.HasKey(
            permission =>
                permission.Id);

        builder.Property(
                permission =>
                    permission.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(
                permission =>
                    permission.Description)
            .HasMaxLength(300);

        builder.HasIndex(
                permission =>
                    permission.Name)
            .IsUnique();
    }
}