namespace IdentityService.API.Constants;

/// <summary>
/// Application role constants used for authorization.
/// These roles are defined in Azure AD App Roles.
/// </summary>
public static class AppRoles
{
    public const string Manager = "Manager";
    public const string StoreCustomer = "StoreCustomer";
}

/// <summary>
/// Application permission constants for fine-grained authorization.
/// Permissions are derived from user roles.
/// </summary>
public static class AppPermissions
{
    public const string Read = "Read";
    public const string Create = "Create";
    public const string Update = "Update";
    public const string Delete = "Delete";
}
