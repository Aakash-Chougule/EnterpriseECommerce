namespace EnterpriseECommerce.Application.DTOs;

/// <summary>
/// Response returned after successful authentication.
/// </summary>
public class AuthResponseDto
{
    public Guid UserId { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;

    /// <summary>
    /// JWT access token used to access protected APIs.
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;

    public bool IsMainAdmin { get; set; }

    public List<string> Permissions { get; set; } = [];

    /// <summary>
    /// Token expiration time in UTC.
    /// </summary>
    public DateTime ExpiresAt { get; set; }
}