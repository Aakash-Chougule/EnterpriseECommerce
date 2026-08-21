using EnterpriseECommerce.Application.DTOs;
using EnterpriseECommerce.Application.Interfaces;
using EnterpriseECommerce.Domain.Entities;

namespace EnterpriseECommerce.Application.Services;

public class AdminUserService
{
    private readonly IUserRepository
        _userRepository;

    private readonly IRoleRepository
        _roleRepository;

    private readonly IPermissionRepository
        _permissionRepository;

    private readonly IPasswordHasher
        _passwordHasher;

    public AdminUserService(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IPermissionRepository permissionRepository,
        IPasswordHasher passwordHasher)
    {
        _userRepository =
            userRepository;

        _roleRepository =
            roleRepository;

        _permissionRepository =
            permissionRepository;

        _passwordHasher =
            passwordHasher;
    }

    // ============================================================
    // GET USERS
    // ============================================================

    public async Task<IReadOnlyList<UserProfileDto>>
        GetAllUsersAsync()
    {
        var users =
            await _userRepository
                .GetAllAsync();

        return users
            .Select(
                Map)
            .ToList();
    }

    public async Task<UserProfileDto>
        GetUserAsync(
            Guid userId)
    {
        var user =
            await GetRequiredUserAsync(
                userId);

        return Map(
            user);
    }

    // ============================================================
    // UPDATE ANY USER
    // ============================================================

    public async Task<UserProfileDto>
        UpdateUserAsync(
            Guid actorUserId,
            Guid targetUserId,
            AdminUpdateUserRequest request)
    {
        var actor =
            await GetRequiredUserAsync(
                actorUserId);

        var target =
            await GetRequiredUserAsync(
                targetUserId);

        EnsureCanModifyUser(
            actor,
            target);

        var email =
            request.Email
                .Trim()
                .ToLowerInvariant();

        if (await _userRepository
            .ExistsByEmailExceptUserAsync(
                email,
                target.Id))
        {
            throw new InvalidOperationException(
                "Another user already uses this email address.");
        }

        target.UpdateProfile(
            request.FirstName,
            request.LastName,
            email,
            request.PhoneNumber);

        if (request.IsActive)
        {
            target.Activate();
        }
        else
        {
            target.Deactivate();
        }

        await _userRepository
            .UpdateAsync(
                target);

        return Map(
            target);
    }

    // ============================================================
    // CREATE ADMIN
    // ============================================================

    public async Task<UserProfileDto>
        CreateAdminAsync(
            CreateAdminRequest request)
    {
        var email =
            request.Email
                .Trim()
                .ToLowerInvariant();

        if (await _userRepository
            .ExistsByEmailAsync(
                email))
        {
            throw new InvalidOperationException(
                "A user with this email address already exists.");
        }

        var adminRole =
            await _roleRepository
                .GetByNameAsync(
                    "Admin")
            ?? throw new InvalidOperationException(
                "Admin role is not configured.");

        var passwordHash =
            _passwordHasher.Hash(
                request.Password);

        var user =
            new User(
                request.FirstName,
                request.LastName,
                email,
                passwordHash,
                adminRole.Id,
                request.PhoneNumber);

        await AssignPermissionsAsync(
            user,
            request.Permissions);

        await _userRepository
            .AddAsync(
                user);

        return Map(
            user);
    }

    // ============================================================
    // PROMOTE EXISTING USER
    // ============================================================

    public async Task<UserProfileDto>
        PromoteToAdminAsync(
            Guid targetUserId,
            PromoteUserToAdminRequest request)
    {
        var user =
            await GetRequiredUserAsync(
                targetUserId);

        if (user.IsMainAdmin)
        {
            return Map(
                user);
        }

        var adminRole =
            await _roleRepository
                .GetByNameAsync(
                    "Admin")
            ?? throw new InvalidOperationException(
                "Admin role is not configured.");

        user.AssignRole(
            adminRole);

        user.ClearPermissions();

        await AssignPermissionsAsync(
            user,
            request.Permissions);

        await _userRepository
            .UpdateAsync(
                user);

        return Map(
            user);
    }

    // ============================================================
    // DEMOTE ADMIN
    // ============================================================

    public async Task<UserProfileDto>
        DemoteAdminAsync(
            Guid targetUserId)
    {
        var user =
            await GetRequiredUserAsync(
                targetUserId);

        if (user.IsMainAdmin)
        {
            throw new InvalidOperationException(
                "Main Admin cannot be demoted.");
        }

        var customerRole =
            await _roleRepository
                .GetByNameAsync(
                    "Customer")
            ?? throw new InvalidOperationException(
                "Customer role is not configured.");

        user.AssignRole(
            customerRole);

        user.ClearPermissions();

        await _userRepository
            .UpdateAsync(
                user);

        return Map(
            user);
    }

    // ============================================================
    // PERMISSIONS
    // ============================================================

    public async Task<UserProfileDto>
        SetPermissionsAsync(
            Guid targetUserId,
            UpdateAdminPermissionsRequest request)
    {
        var user =
            await GetRequiredUserAsync(
                targetUserId);

        if (user.IsMainAdmin)
        {
            throw new InvalidOperationException(
                "Main Admin permissions cannot be restricted.");
        }

        if (!string.Equals(
            user.Role.Name,
            "Admin",
            StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Permissions can only be assigned to Admin users.");
        }

        user.ClearPermissions();

        await AssignPermissionsAsync(
            user,
            request.Permissions);

        await _userRepository
            .UpdateAsync(
                user);

        return Map(
            user);
    }

    public async Task<IReadOnlyList<string>>
        GetAvailablePermissionsAsync()
    {
        var permissions =
            await _permissionRepository
                .GetAllAsync();

        return permissions
            .Select(
                permission =>
                    permission.Name)
            .OrderBy(
                name =>
                    name)
            .ToList();
    }

    // ============================================================
    // HELPERS
    // ============================================================

    private async Task AssignPermissionsAsync(
        User user,
        IEnumerable<string> names)
    {
        var requestedNames =
            names
                .Where(
                    value =>
                        !string.IsNullOrWhiteSpace(
                            value))
                .Distinct(
                    StringComparer.OrdinalIgnoreCase)
                .ToList();

        var permissions =
            await _permissionRepository
                .GetByNamesAsync(
                    requestedNames);

        if (permissions.Count !=
            requestedNames.Count)
        {
            throw new ArgumentException(
                "One or more permissions are invalid.");
        }

        foreach (var permission in
                 permissions)
        {
            user.AddPermission(
                permission);
        }
    }

    private async Task<User>
        GetRequiredUserAsync(
            Guid userId)
    {
        return await _userRepository
                   .GetByIdAsync(
                       userId)
               ?? throw new KeyNotFoundException(
                   "User not found.");
    }

    private static void EnsureCanModifyUser(
        User actor,
        User target)
    {
        if (!target.IsMainAdmin)
        {
            return;
        }

        if (actor.Id ==
                target.Id &&
            actor.IsMainAdmin)
        {
            return;
        }

        throw new UnauthorizedAccessException(
            "Only the Main Admin can modify the Main Admin account.");
    }

    private static UserProfileDto Map(
        User user)
    {
        return new UserProfileDto
        {
            Id =
                user.Id,

            FirstName =
                user.FirstName,

            LastName =
                user.LastName,

            Email =
                user.Email,

            PhoneNumber =
                user.PhoneNumber,

            Role =
                user.Role.Name,

            IsActive =
                user.IsActive,

            IsMainAdmin =
                user.IsMainAdmin,

            Permissions =
                user.UserPermissions
                    .Select(
                        item =>
                            item.Permission.Name)
                    .OrderBy(
                        name =>
                            name)
                    .ToList()
        };
    }
}