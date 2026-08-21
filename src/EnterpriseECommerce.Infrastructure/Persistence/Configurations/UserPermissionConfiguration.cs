using EnterpriseECommerce.Domain.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnterpriseECommerce.Infrastructure.Persistence.Configurations;

public class UserPermissionConfiguration :
    IEntityTypeConfiguration<UserPermission>
{
    public void Configure(
        EntityTypeBuilder<UserPermission> builder)
    {
        builder.ToTable(
            "UserPermissions");

        builder.HasKey(
            item =>
                new
                {
                    item.UserId,
                    item.PermissionId
                });

        builder.HasOne(
                item =>
                    item.User)
            .WithMany(
                user =>
                    user.UserPermissions)
            .HasForeignKey(
                item =>
                    item.UserId)
            .OnDelete(
                DeleteBehavior.Cascade);

        builder.HasOne(
                item =>
                    item.Permission)
            .WithMany()
            .HasForeignKey(
                item =>
                    item.PermissionId)
            .OnDelete(
                DeleteBehavior.Cascade);
    }
}