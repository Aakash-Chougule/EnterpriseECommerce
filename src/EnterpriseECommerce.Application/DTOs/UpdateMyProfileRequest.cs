namespace EnterpriseECommerce.Application.DTOs;

public class UpdateMyProfileRequest
{
    public string FirstName { get; set; } =
        string.Empty;

    public string LastName { get; set; } =
        string.Empty;

    public string Email { get; set; } =
        string.Empty;

    public string? PhoneNumber { get; set; }
}