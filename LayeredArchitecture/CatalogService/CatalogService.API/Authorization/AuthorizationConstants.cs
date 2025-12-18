namespace CatalogService.API.Authorization;

public static class Permissions
{
    public const string Read = "Read";
    public const string Create = "Create";
    public const string Update = "Update";
    public const string Delete = "Delete";
}

public static class Policies
{
    public const string CanRead = "CanRead";
    public const string CanCreate = "CanCreate";
    public const string CanUpdate = "CanUpdate";
    public const string CanDelete = "CanDelete";
}
