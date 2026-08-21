using EnterpriseECommerce.Domain.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnterpriseECommerce.Infrastructure.Persistence.Configurations;

public class UserConfiguration :
    IEntityTypeConfiguration<User>
{
    public void Configure(
        EntityTypeBuilder<User> builder)
    {
        builder.ToTable(
            "Users");

        builder.HasKey(
            user =>
                user.Id);

        builder.Property(
                user =>
                    user.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(
                user =>
                    user.LastName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(
                user =>
                    user.Email)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(
                user =>
                    user.PasswordHash)
            .IsRequired();

        builder.Property(
                user =>
                    user.PhoneNumber)
            .HasMaxLength(20);

        builder.Property(
                user =>
                    user.IsActive)
            .IsRequired();

        builder.Property(
                user =>
                    user.IsMainAdmin)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(
                user =>
                    user.CreatedAt)
            .IsRequired();

        builder.HasIndex(
                user =>
                    user.Email)
            .IsUnique();

        builder.HasOne(
                user =>
                    user.Role)
            .WithMany()
            .HasForeignKey(
                user =>
                    user.RoleId)
            .OnDelete(
                DeleteBehavior.Restrict);

        builder.HasMany(
                user =>
                    user.UserPermissions)
            .WithOne(
                permission =>
                    permission.User)
            .HasForeignKey(
                permission =>
                    permission.UserId)
            .OnDelete(
                DeleteBehavior.Cascade);
    }
}