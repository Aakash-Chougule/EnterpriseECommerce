using EnterpriseECommerce.Domain.Entities;

namespace EnterpriseECommerce.Application.Interfaces;

/// <summary>
/// Defines JWT token generation operations.
///
/// The Application layer depends on this abstraction rather than
/// directly depending on the JWT implementation.
/// </summary>
public interface IJwtTokenService
{
    /// <summary>
    /// Generates an access token for the specified user.
    /// </summary>
    string GenerateToken(User user);

    /// <summary>
    /// Gets the expiration time of the generated access token.
    /// </summary>
    DateTime GetExpirationTime();
}