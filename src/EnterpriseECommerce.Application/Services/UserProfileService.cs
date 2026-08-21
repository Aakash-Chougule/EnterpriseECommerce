using EnterpriseECommerce.Application.DTOs;
using EnterpriseECommerce.Application.Interfaces;
using EnterpriseECommerce.Domain.Entities;

namespace EnterpriseECommerce.Application.Services;

public class UserProfileService
{
    private readonly IUserRepository
        _userRepository;

    private readonly IPasswordHasher
        _passwordHasher;

    public UserProfileService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher)
    {
        _userRepository =
            userRepository;

        _passwordHasher =
            passwordHasher;
    }

    // ============================================================
    // GET PROFILE
    // ============================================================

    public async Task<UserProfileDto>
        GetProfileAsync(
            Guid userId)
    {
        var user =
            await GetUserAsync(
                userId);

        return Map(
            user);
    }

    // ============================================================
    // UPDATE PROFILE
    // ============================================================

    public async Task<UserProfileDto>
        UpdateProfileAsync(
            Guid userId,
            UpdateMyProfileRequest request)
    {
        if (request is null)
        {
            throw new ArgumentNullException(
                nameof(request));
        }

        if (string.IsNullOrWhiteSpace(
            request.FirstName))
        {
            throw new ArgumentException(
                "First name is required.");
        }

        if (string.IsNullOrWhiteSpace(
            request.LastName))
        {
            throw new ArgumentException(
                "Last name is required.");
        }

        if (string.IsNullOrWhiteSpace(
            request.Email))
        {
            throw new ArgumentException(
                "Email is required.");
        }

        var user =
            await GetUserAsync(
                userId);

        var email =
            request.Email
                .Trim()
                .ToLowerInvariant();

        var emailExists =
            await _userRepository
                .ExistsByEmailExceptUserAsync(
                    email,
                    userId);

        if (emailExists)
        {
            throw new InvalidOperationException(
                "Another user already uses this email address.");
        }

        user.UpdateProfile(
            request.FirstName.Trim(),
            request.LastName.Trim(),
            email,
            request.PhoneNumber?.Trim());

        await _userRepository
            .UpdateAsync(
                user);

        return Map(
            user);
    }

    // ============================================================
    // CHANGE PASSWORD
    // ============================================================

    public async Task ChangePasswordAsync(
        Guid userId,
        ChangePasswordRequest request)
    {
        if (request is null)
        {
            throw new ArgumentNullException(
                nameof(request));
        }

        if (string.IsNullOrWhiteSpace(
            request.CurrentPassword))
        {
            throw new ArgumentException(
                "Current password is required.");
        }

        if (string.IsNullOrWhiteSpace(
            request.NewPassword))
        {
            throw new ArgumentException(
                "New password is required.");
        }

        if (request.NewPassword.Length < 8)
        {
            throw new ArgumentException(
                "New password must contain at least 8 characters.");
        }

        if (request.CurrentPassword ==
            request.NewPassword)
        {
            throw new ArgumentException(
                "New password must be different from the current password.");
        }

        var user =
            await GetUserAsync(
                userId);

        var valid =
            _passwordHasher.Verify(
                request.CurrentPassword,
                user.PasswordHash);

        if (!valid)
        {
            throw new UnauthorizedAccessException(
                "Current password is incorrect.");
        }

        var hash =
            _passwordHasher.Hash(
                request.NewPassword);

        user.ChangePasswordHash(
            hash);

        await _userRepository
            .UpdateAsync(
                user);
    }

    // ============================================================
    // LOAD USER
    // ============================================================

    private async Task<User>
        GetUserAsync(
            Guid userId)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException(
                "UserId is required.");
        }

        var user =
            await _userRepository
                .GetByIdAsync(
                    userId);

        if (user is null)
        {
            throw new KeyNotFoundException(
                "User not found.");
        }

        return user;
    }

    // ============================================================
    // MAPPING
    // ============================================================

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