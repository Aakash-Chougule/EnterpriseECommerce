using EnterpriseECommerce.Application.DTOs;

namespace EnterpriseECommerce.Application.Interfaces;

/// <summary>
/// Defines authentication operations for the application.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Registers a new customer account.
    /// </summary>
    Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request);

    /// <summary>
    /// Authenticates an existing user and generates an access token.
    /// </summary>
    Task<AuthResponseDto> LoginAsync(LoginRequestDto request);
}