using EnterpriseECommerce.Application.DTOs;
using EnterpriseECommerce.Application.Interfaces;
using EnterpriseECommerce.Domain.Entities;

namespace EnterpriseECommerce.Application.Services;

/// <summary>
/// Handles application authentication and user registration.
///
/// This service coordinates repositories, password hashing,
/// and JWT generation. It does not directly access PostgreSQL
/// or depend on ASP.NET Core authentication infrastructure.
/// </summary>
public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;

    public AuthService(
       IUserRepository userRepository,
       IRoleRepository roleRepository,
       IPasswordHasher passwordHasher,
       IJwtTokenService jwtTokenService)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
    }

    /// <summary>
    /// Registers a new customer account.
    /// </summary>
    public async Task<AuthResponseDto> RegisterAsync(
        RegisterRequestDto request)
    {
        // --------------------------------------------------------
        // Validate required registration fields.
        // --------------------------------------------------------

        if (string.IsNullOrWhiteSpace(request.FirstName))
            throw new ArgumentException("First name is required.");

        if (string.IsNullOrWhiteSpace(request.LastName))
            throw new ArgumentException("Last name is required.");

        if (string.IsNullOrWhiteSpace(request.Email))
            throw new ArgumentException("Email is required.");

        if (string.IsNullOrWhiteSpace(request.Password))
            throw new ArgumentException("Password is required.");

        // Normalize the email so that:
        //
        // Aakash@Example.com
        //
        // and
        //
        // aakash@example.com
        //
        // are treated consistently.
        var email = request.Email.Trim().ToLowerInvariant();

        // --------------------------------------------------------
        // Prevent duplicate accounts.
        // --------------------------------------------------------

        var existingUser =
            await _userRepository.GetByEmailAsync(email);

        if (existingUser is not null)
        {
            throw new InvalidOperationException(
                "A user with this email address already exists.");
        }

        // --------------------------------------------------------
        // IMPORTANT:
        //
        // Registration must NEVER allow the client to choose
        // an administrative role.
        //
        // New registrations will be assigned the Customer role.
        // --------------------------------------------------------

        // --------------------------------------------------------
        // Retrieve the default Customer role.
        //
        // Users registering through the public API must always
        // receive the Customer role.
        // --------------------------------------------------------

        var customerRole =
            await _roleRepository.GetByNameAsync("Customer");

        if (customerRole is null)
        {
            throw new InvalidOperationException(
                "Customer role has not been configured.");
        }

        // --------------------------------------------------------
        // Hash the password before creating the User entity.
        //
        // Plain-text passwords are NEVER stored in PostgreSQL.
        // --------------------------------------------------------

        var passwordHash =
            _passwordHasher.Hash(request.Password);

        // --------------------------------------------------------
        // Create the domain User entity.
        // --------------------------------------------------------

        var user = new User(
            request.FirstName.Trim(),
            request.LastName.Trim(),
            email,
            passwordHash,
            customerRole.Id,
            request.PhoneNumber?.Trim());

        // --------------------------------------------------------
        // Persist the user through the repository.
        // --------------------------------------------------------

        await _userRepository.AddAsync(user);

        // --------------------------------------------------------
        // Generate JWT for the newly registered user.
        // --------------------------------------------------------

        var accessToken =
            _jwtTokenService.GenerateToken(user);

        var expiresAt =
            _jwtTokenService.GetExpirationTime();

        // --------------------------------------------------------
        // Return authentication response.
        // --------------------------------------------------------

        return new AuthResponseDto
        {
            UserId = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Role = customerRole.Name,
            AccessToken = accessToken,
            ExpiresAt = expiresAt
        };
    }

    /// <summary>
    /// Authenticates an existing user.
    /// </summary>
    public async Task<AuthResponseDto> LoginAsync(
        LoginRequestDto request)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        // --------------------------------------------------------
        // Find the user and load their Role.
        // --------------------------------------------------------

        var user =
            await _userRepository.GetByEmailAsync(email);

        if (user is null)
        {
            throw new UnauthorizedAccessException(
                "Invalid email or password.");
        }

        // --------------------------------------------------------
        // Check whether the account is active.
        // --------------------------------------------------------

        if (!user.IsActive)
        {
            throw new UnauthorizedAccessException(
                "This user account is inactive.");
        }

        // --------------------------------------------------------
        // Verify the supplied password against the stored
        // BCrypt password hash.
        // --------------------------------------------------------

        var passwordValid =
            _passwordHasher.Verify(
                request.Password,
                user.PasswordHash);

        if (!passwordValid)
        {
            // Do not reveal whether the email or password
            // was incorrect.
            throw new UnauthorizedAccessException(
                "Invalid email or password.");
        }

        // --------------------------------------------------------
        // Generate JWT.
        // --------------------------------------------------------

        var token =
            _jwtTokenService.GenerateToken(user);

        var expiration =
            _jwtTokenService.GetExpirationTime();

        return new AuthResponseDto
        {
            UserId = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Role = user.Role.Name,
            AccessToken = token,
            ExpiresAt = expiration
        };
    }
}