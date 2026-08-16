namespace EnterpriseECommerce.Application.DTOs;

/// <summary>
/// Represents a shopping cart returned by the API.
/// </summary>
public class CartDto
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public List<CartItemDto> Items { get; set; } = new();

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}