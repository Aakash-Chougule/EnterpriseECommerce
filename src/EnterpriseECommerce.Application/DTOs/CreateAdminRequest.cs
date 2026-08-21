namespace EnterpriseECommerce.Application.DTOs;

public class CreateAdminRequest
{
    public string FirstName { get; set; } =
        string.Empty;

    public string LastName { get; set; } =
        string.Empty;

    public string Email { get; set; } =
        string.Empty;

    public string Password { get; set; } =
        string.Empty;

    public string? PhoneNumber { get; set; }

    public List<string> Permissions { get; set; } =
        [];
}