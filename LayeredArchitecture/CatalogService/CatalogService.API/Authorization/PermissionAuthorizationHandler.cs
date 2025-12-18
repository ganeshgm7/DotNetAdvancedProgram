using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace CatalogService.API.Authorization;

public class PermissionRequirement : IAuthorizationRequirement
{
    public string Permission { get; }

    public PermissionRequirement(string permission)
    {
        Permission = permission;
    }
}

public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly ILogger<PermissionAuthorizationHandler> _logger;

    public PermissionAuthorizationHandler(ILogger<PermissionAuthorizationHandler> logger)
    {
        _logger = logger;
    }

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        var username = context.User.Identity?.Name ?? "Unknown";

        // Check if user has the required permission claim
        bool hasPermission = context.User.HasClaim(c => c.Type == "permission" && c.Value == requirement.Permission);

        if (hasPermission)
        {
            _logger.LogDebug("User {Username} has permission {Permission}", username, requirement.Permission);
            context.Succeed(requirement);
        }
        else
        {
            _logger.LogWarning("User {Username} lacks permission {Permission}. Available permissions: {Permissions}",
                username,
                requirement.Permission,
                string.Join(", ", context.User.Claims.Where(c => c.Type == "permission").Select(c => c.Value)));
        }

        return Task.CompletedTask;
    }
}
