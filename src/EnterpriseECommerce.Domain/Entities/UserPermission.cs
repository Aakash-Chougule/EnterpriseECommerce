namespace EnterpriseECommerce.Domain.Entities;

public class UserPermission
{
    public Guid UserId { get; private set; }

    public Guid PermissionId { get; private set; }

    public User User { get; private set; } =
        null!;

    public Permission Permission { get; private set; } =
        null!;

    private UserPermission()
    {
    }

    public UserPermission(
        Guid userId,
        Guid permissionId)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException(
                "UserId is required.");
        }

        if (permissionId == Guid.Empty)
        {
            throw new ArgumentException(
                "PermissionId is required.");
        }

        UserId =
            userId;

        PermissionId =
            permissionId;
    }
}