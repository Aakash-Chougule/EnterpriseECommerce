namespace EnterpriseECommerce.Domain.Entities;

public class User
{
    public Guid Id { get; private set; }

    public string FirstName { get; private set; } = string.Empty;

    public string LastName { get; private set; } = string.Empty;

    public string Email { get; private set; } = string.Empty;

    public string PasswordHash { get; private set; } = string.Empty;

    public string? PhoneNumber { get; private set; }

    public bool IsActive { get; private set; }

    // ============================================================
    // MAIN ADMIN
    // ============================================================

    public bool IsMainAdmin { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime? UpdatedAt { get; private set; }

    // ============================================================
    // ROLE
    // ============================================================

    public Guid RoleId { get; private set; }

    public Role Role { get; private set; } = null!;

    // ============================================================
    // PERMISSIONS
    // ============================================================

    public ICollection<UserPermission> UserPermissions
    {
        get;
        private set;
    } = new List<UserPermission>();

    // ============================================================
    // EF CORE
    // ============================================================

    private User()
    {
    }

    // ============================================================
    // CONSTRUCTOR
    // ============================================================

    public User(
        string firstName,
        string lastName,
        string email,
        string passwordHash,
        Guid roleId,
        string? phoneNumber = null)
    {
        if (string.IsNullOrWhiteSpace(firstName))
        {
            throw new ArgumentException(
                "First name is required.");
        }

        if (string.IsNullOrWhiteSpace(lastName))
        {
            throw new ArgumentException(
                "Last name is required.");
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException(
                "Email is required.");
        }

        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            throw new ArgumentException(
                "Password hash is required.");
        }

        if (roleId == Guid.Empty)
        {
            throw new ArgumentException(
                "Role is required.");
        }

        Id = Guid.NewGuid();

        FirstName =
            firstName.Trim();

        LastName =
            lastName.Trim();

        Email =
            email.Trim().ToLowerInvariant();

        PasswordHash =
            passwordHash;

        RoleId =
            roleId;

        PhoneNumber =
            string.IsNullOrWhiteSpace(phoneNumber)
                ? null
                : phoneNumber.Trim();

        IsActive =
            true;

        IsMainAdmin =
            false;

        CreatedAt =
            DateTime.UtcNow;
    }

    // ============================================================
    // UPDATE PROFILE
    // ============================================================

    public void UpdateProfile(
        string firstName,
        string lastName,
        string email,
        string? phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(firstName))
        {
            throw new ArgumentException(
                "First name is required.");
        }

        if (string.IsNullOrWhiteSpace(lastName))
        {
            throw new ArgumentException(
                "Last name is required.");
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException(
                "Email is required.");
        }

        FirstName =
            firstName.Trim();

        LastName =
            lastName.Trim();

        Email =
            email.Trim().ToLowerInvariant();

        PhoneNumber =
            string.IsNullOrWhiteSpace(phoneNumber)
                ? null
                : phoneNumber.Trim();

        UpdatedAt =
            DateTime.UtcNow;
    }

    // ============================================================
    // PASSWORD
    // ============================================================

    public void ChangePasswordHash(
        string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            throw new ArgumentException(
                "Password hash is required.");
        }

        PasswordHash =
            passwordHash;

        UpdatedAt =
            DateTime.UtcNow;
    }

    // ============================================================
    // ROLE
    // ============================================================

    public void AssignRole(
        Role role)
    {
        ArgumentNullException.ThrowIfNull(
            role);

        Role =
            role;

        RoleId =
            role.Id;

        UpdatedAt =
            DateTime.UtcNow;
    }

    // ============================================================
    // MAIN ADMIN
    // ============================================================

    public void MarkAsMainAdmin()
    {
        IsMainAdmin =
            true;

        UpdatedAt =
            DateTime.UtcNow;
    }

    // ============================================================
    // STATUS
    // ============================================================

    public void Activate()
    {
        IsActive =
            true;

        UpdatedAt =
            DateTime.UtcNow;
    }

    public void Deactivate()
    {
        if (IsMainAdmin)
        {
            throw new InvalidOperationException(
                "Main Admin cannot be deactivated.");
        }

        IsActive =
            false;

        UpdatedAt =
            DateTime.UtcNow;
    }

    // ============================================================
    // PERMISSIONS
    // ============================================================

    public void AddPermission(
        Permission permission)
    {
        ArgumentNullException.ThrowIfNull(
            permission);

        if (UserPermissions.Any(
            item =>
                item.PermissionId ==
                permission.Id))
        {
            return;
        }

        UserPermissions.Add(
            new UserPermission(
                Id,
                permission.Id));
    }

    public void RemovePermission(
        Guid permissionId)
    {
        var existing =
            UserPermissions
                .FirstOrDefault(
                    item =>
                        item.PermissionId ==
                        permissionId);

        if (existing is null)
        {
            return;
        }

        UserPermissions.Remove(
            existing);
    }

    public void ClearPermissions()
    {
        UserPermissions.Clear();

        UpdatedAt =
            DateTime.UtcNow;
    }
}