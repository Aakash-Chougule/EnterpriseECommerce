namespace EnterpriseECommerce.Application.DTOs;

public class UserProfileDto
{
    public Guid Id { get; set; }

    public string FirstName { get; set; } =
        string.Empty;

    public string LastName { get; set; } =
        string.Empty;

    public string Email { get; set; } =
        string.Empty;

    public string? PhoneNumber { get; set; }

    public string Role { get; set; } =
        string.Empty;

    public bool IsActive { get; set; }

    public bool IsMainAdmin { get; set; }

    public List<string> Permissions { get; set; } =
        [];
}