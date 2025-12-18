namespace IdentityService.API.Configuration;

public class AzureAdSettings
{
    public string Instance { get; set; } = "https://login.microsoftonline.com/";
    public string Domain { get; set; } = null!;
    public string TenantId { get; set; } = null!;
    public string ClientId { get; set; } = null!;
    public string ClientSecret { get; set; } = null!;
    public string CallbackPath { get; set; } = "/signin-oidc";
    public string Audience { get; set; } = null!;
    public AppRoleMappings RoleMappings { get; set; } = new();
}

public class AppRoleMappings
{
    public string ManagerRoleId { get; set; } = null!;
    public string StoreCustomerRoleId { get; set; } = null!;
}
