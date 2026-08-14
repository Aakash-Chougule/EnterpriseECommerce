using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using EnterpriseECommerce.Application.Interfaces;
using EnterpriseECommerce.Domain.Entities;

using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace EnterpriseECommerce.Infrastructure.Security;

/// <summary>
/// Generates JWT access tokens for authenticated users.
///
/// This implementation belongs to Infrastructure because JWT
/// is an external/security implementation detail. The Application
/// layer only depends on IJwtTokenService.
/// </summary>
public class JwtTokenService : IJwtTokenService
{
    private readonly IConfiguration _configuration;

    public JwtTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <summary>
    /// Generates a signed JWT containing the user's identity
    /// and authorization role.
    /// </summary>
    public string GenerateToken(User user)
    {
        var jwtSettings = _configuration.GetSection("Jwt");

        var secretKey = jwtSettings["SecretKey"]
            ?? throw new InvalidOperationException(
                "JWT SecretKey is not configured.");

        var issuer = jwtSettings["Issuer"]
            ?? throw new InvalidOperationException(
                "JWT Issuer is not configured.");

        var audience = jwtSettings["Audience"]
            ?? throw new InvalidOperationException(
                "JWT Audience is not configured.");

        var expirationMinutes =
            int.Parse(jwtSettings["ExpirationMinutes"] ?? "60");

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(secretKey));

        var credentials = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256);

        // Claims represent information about the authenticated user.
        var claims = new List<Claim>
        {
            // Unique user identifier.
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),

            // User email.
            new(JwtRegisteredClaimNames.Email, user.Email),

            // ASP.NET Core uses this claim for role-based authorization.
            new(ClaimTypes.Role, user.Role.Name),

            // Useful for identifying the user inside the application.
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),

            new(ClaimTypes.Name, $"{user.FirstName} {user.LastName}")
        };

        var expiration = DateTime.UtcNow.AddMinutes(expirationMinutes);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expiration,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// Returns the expiration time configured for access tokens.
    /// </summary>
    public DateTime GetExpirationTime()
    {
        var expirationMinutes =
            int.Parse(
                _configuration["Jwt:ExpirationMinutes"] ?? "60");

        return DateTime.UtcNow.AddMinutes(expirationMinutes);
    }
}