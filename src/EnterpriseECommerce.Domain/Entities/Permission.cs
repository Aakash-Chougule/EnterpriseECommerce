namespace EnterpriseECommerce.Domain.Entities;

public class Permission
{
    public Guid Id { get; private set; }

    public string Name { get; private set; } =
        string.Empty;

    public string? Description { get; private set; }

    private Permission()
    {
    }

    public Permission(
        string name,
        string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "Permission name is required.");
        }

        Id =
            Guid.NewGuid();

        Name =
            name.Trim();

        Description =
            description?.Trim();
    }
}