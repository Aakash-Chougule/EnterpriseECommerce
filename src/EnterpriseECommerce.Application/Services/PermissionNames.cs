namespace EnterpriseECommerce.Application.Security;

public static class PermissionNames
{
    public const string ManageProducts =
        "ManageProducts";

    public const string ManageCategories =
        "ManageCategories";

    public const string ManageInventory =
        "ManageInventory";

    public const string ManageOrders =
        "ManageOrders";

    public const string ManagePayments =
        "ManagePayments";

    public const string ManageUsers =
        "ManageUsers";

    public const string ManageAdmins =
        "ManageAdmins";

    public const string ViewReports =
        "ViewReports";

    public static readonly string[] All =
    [
        ManageProducts,
        ManageCategories,
        ManageInventory,
        ManageOrders,
        ManagePayments,
        ManageUsers,
        ManageAdmins,
        ViewReports
    ];
}