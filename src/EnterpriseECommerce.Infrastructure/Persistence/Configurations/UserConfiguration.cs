using EnterpriseECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnterpriseECommerce.Infrastructure.Persistence.Configurations;

/// <summary>
/// Defines the PostgreSQL mapping and database constraints for the User entity.
/// </summary>
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        // --------------------------------------------------------
        // Table configuration
        // --------------------------------------------------------

        builder.ToTable("Users");

        // --------------------------------------------------------
        // Primary key
        // --------------------------------------------------------

        builder.HasKey(user => user.Id);

        // --------------------------------------------------------
        // User properties
        // --------------------------------------------------------

        builder.Property(user => user.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(user => user.LastName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(user => user.Email)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(user => user.PasswordHash)
            .IsRequired();

        builder.Property(user => user.PhoneNumber)
            .HasMaxLength(20);

        builder.Property(user => user.IsActive)
            .IsRequired();

        builder.Property(user => user.CreatedAt)
            .IsRequired();

        // --------------------------------------------------------
        // Email uniqueness
        // --------------------------------------------------------
        // Prevents multiple users from registering with
        // the same email address.
        // --------------------------------------------------------

        builder.HasIndex(user => user.Email)
            .IsUnique();

        // --------------------------------------------------------
        // User -> Role relationship
        // --------------------------------------------------------
        // Many users can have the same role.
        //
        // Example:
        //
        // Admin Role
        //    ├── User 1
        //    ├── User 2
        //    └── User 3
        //
        // The RoleId column in Users acts as the foreign key.
        // --------------------------------------------------------

        builder.HasOne(user => user.Role)
            .WithMany()
            .HasForeignKey(user => user.RoleId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}