using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using EnterpriseECommerce.Application.Interfaces;
using EnterpriseECommerce.Domain.Entities;

using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace EnterpriseECommerce.Infrastructure.Security;

public class JwtTokenService :
    IJwtTokenService
{
    private readonly IConfiguration
        _configuration;

    public JwtTokenService(
        IConfiguration configuration)
    {
        _configuration =
            configuration;
    }

    public string GenerateToken(
        User user)
    {
        var jwtSettings =
            _configuration
                .GetSection(
                    "Jwt");

        var secretKey =
            jwtSettings[
                "SecretKey"]
            ?? throw new InvalidOperationException(
                "JWT SecretKey is not configured.");

        var issuer =
            jwtSettings[
                "Issuer"]
            ?? throw new InvalidOperationException(
                "JWT Issuer is not configured.");

        var audience =
            jwtSettings[
                "Audience"]
            ?? throw new InvalidOperationException(
                "JWT Audience is not configured.");

        var expirationMinutes =
            int.Parse(
                jwtSettings[
                    "ExpirationMinutes"]
                ?? "60");

        var key =
            new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    secretKey));

        var credentials =
            new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);

        var claims =
            new List<Claim>
            {
                new(
                    JwtRegisteredClaimNames.Sub,
                    user.Id.ToString()),

                new(
                    JwtRegisteredClaimNames.Email,
                    user.Email),

                new(
                    ClaimTypes.Role,
                    user.Role.Name),

                new(
                    ClaimTypes.NameIdentifier,
                    user.Id.ToString()),

                new(
                    ClaimTypes.Name,
                    $"{user.FirstName} {user.LastName}"),

                new(
                    "is_main_admin",
                    user.IsMainAdmin
                        ? "true"
                        : "false")
            };

        foreach (var permission in
                 user.UserPermissions)
        {
            claims.Add(
                new Claim(
                    "permission",
                    permission.Permission.Name));
        }

        var expiration =
            DateTime.UtcNow.AddMinutes(
                expirationMinutes);

        var token =
            new JwtSecurityToken(
                issuer:
                    issuer,

                audience:
                    audience,

                claims:
                    claims,

                expires:
                    expiration,

                signingCredentials:
                    credentials);

        return new JwtSecurityTokenHandler()
            .WriteToken(
                token);
    }

    public DateTime GetExpirationTime()
    {
        var expirationMinutes =
            int.Parse(
                _configuration[
                    "Jwt:ExpirationMinutes"]
                ?? "60");

        return DateTime.UtcNow.AddMinutes(
            expirationMinutes);
    }
}