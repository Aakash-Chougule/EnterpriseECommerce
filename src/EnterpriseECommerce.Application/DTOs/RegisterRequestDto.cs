namespace EnterpriseECommerce.Application.DTOs;

/// <summary>
/// Data received when a new customer registers.
/// </summary>
public class RegisterRequestDto
{
    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; }
}