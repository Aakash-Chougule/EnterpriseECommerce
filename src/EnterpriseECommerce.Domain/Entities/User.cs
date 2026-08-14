namespace EnterpriseECommerce.Domain.Entities;

/// <summary>
/// Represents an application user.
///
/// Authentication-related information such as the password hash
/// is stored with the user, while authorization is handled through
/// the associated Role.
/// </summary>
public class User
{
    public Guid Id { get; private set; }

    public string FirstName { get; private set; } = string.Empty;

    public string LastName { get; private set; } = string.Empty;

    public string Email { get; private set; } = string.Empty;

    /// <summary>
    /// Stores the hashed password.
    ///
    /// The application must NEVER store a plain-text password.
    /// </summary>
    public string PasswordHash { get; private set; } = string.Empty;

    public string? PhoneNumber { get; private set; }

    public bool IsActive { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime? UpdatedAt { get; private set; }

    // ------------------------------------------------------------
    // Authorization
    // ------------------------------------------------------------

    /// <summary>
    /// Foreign key referencing the user's role.
    /// </summary>
    public Guid RoleId { get; private set; }

    /// <summary>
    /// Navigation property used by Entity Framework Core.
    /// </summary>
    public Role Role { get; private set; } = null!;

    // ------------------------------------------------------------
    // EF Core constructor
    // ------------------------------------------------------------

    private User()
    {
    }

    // ------------------------------------------------------------
    // Application constructor
    // ------------------------------------------------------------

    public User(
     string firstName,
     string lastName,
     string email,
     string passwordHash,
     Guid roleId,
     string? phoneNumber = null)
    {
        Id = Guid.NewGuid();

        FirstName = firstName;
        LastName = lastName;
        Email = email;
        PasswordHash = passwordHash;

        RoleId = roleId;

        PhoneNumber = phoneNumber;

        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }

    // ------------------------------------------------------------
    // Domain methods
    // ------------------------------------------------------------

    public void UpdateName(string firstName, string lastName)
    {
        FirstName = firstName;
        LastName = lastName;

        UpdatedAt = DateTime.UtcNow;
    }
    /// <summary>
    /// Associates the user with an application role.
    ///
    /// The RoleId foreign key and navigation property are updated
    /// together so the domain object remains internally consistent.
    /// </summary>
    public void AssignRole(Role role)
    {
        ArgumentNullException.ThrowIfNull(role);

        Role = role;
        RoleId = role.Id;
    }

    public void Deactivate()
    {
        IsActive = false;

        UpdatedAt = DateTime.UtcNow;
    }
}