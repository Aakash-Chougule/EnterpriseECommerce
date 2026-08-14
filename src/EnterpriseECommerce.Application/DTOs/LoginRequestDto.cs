namespace EnterpriseECommerce.Application.DTOs;

/// <summary>
/// Data required to authenticate an existing user.
/// </summary>
public class LoginRequestDto
{
    public string Email { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;
}