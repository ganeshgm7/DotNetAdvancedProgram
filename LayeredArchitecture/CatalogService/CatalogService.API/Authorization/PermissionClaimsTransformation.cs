using System.Security.Claims;
using CatalogService.API.Authorization;
using Microsoft.AspNetCore.Authentication;

namespace CatalogService.API.Authorization;

/// <summary>
/// Claims transformation service that enriches JWT tokens with permission claims based on user roles.
/// This ensures that authorization policies can check for specific permissions.
/// </summary>
public class PermissionClaimsTransformation : IClaimsTransformation
{
    private readonly ILogger<PermissionClaimsTransformation> _logger;

    /// <summary>
    /// Initializes a new instance of the PermissionClaimsTransformation class.
    /// </summary>
    /// <param name="logger">Logger for transformation operations.</param>
    public PermissionClaimsTransformation(ILogger<PermissionClaimsTransformation> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Transforms the claims principal by adding permission claims based on the user's roles.
    /// </summary>
    /// <param name="principal">The original claims principal from the JWT token.</param>
    /// <returns>A new claims principal with additional permission claims.</returns>
    public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        // Clone the principal to avoid modifying the original
        var claimsIdentity = new ClaimsIdentity(principal.Identity);

        // Get user roles from the token
        var roles = principal.Claims
            .Where(c => c.Type == "roles" || 
                       c.Type == ClaimTypes.Role ||
                       c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role")
            .Select(c => c.Value)
            .ToList();

        _logger.LogDebug("Transforming claims for user {Username} with roles: {Roles}", 
            principal.Identity?.Name, string.Join(", ", roles));

        // Check if permissions are already added (avoid duplicate transformation)
        bool hasPermissionClaims = principal.Claims.Any(c => c.Type == "permission");
        
        if (!hasPermissionClaims)
        {
            // Add permission claims based on roles
            var permissions = new HashSet<string>();

            foreach (var role in roles)
            {
                var rolePermissions = GetPermissionsForRole(role);
                foreach (var permission in rolePermissions)
                {
                    permissions.Add(permission);
                }
            }

            // Add each unique permission as a claim
            foreach (var permission in permissions)
            {
                claimsIdentity.AddClaim(new Claim("permission", permission));
                _logger.LogDebug("Added permission claim: {Permission}", permission);
            }

            _logger.LogInformation("Added {Count} permission claims for user {Username}", 
                permissions.Count, principal.Identity?.Name);
        }
        else
        {
            _logger.LogDebug("Permission claims already exist for user {Username}, skipping transformation", 
                principal.Identity?.Name);
        }

        var transformedPrincipal = new ClaimsPrincipal(claimsIdentity);
        return Task.FromResult(transformedPrincipal);
    }

    /// <summary>
    /// Maps a role to its associated permissions.
    /// Manager role: Read, Create, Update, Delete
    /// StoreCustomer role: Read only
    /// </summary>
    /// <param name="role">The role name.</param>
    /// <returns>A collection of permission strings for the role.</returns>
    private static IEnumerable<string> GetPermissionsForRole(string role)
    {
        return role switch
        {
            "Manager" => new[] 
            { 
                Permissions.Read, 
                Permissions.Create, 
                Permissions.Update, 
                Permissions.Delete 
            },
            "StoreCustomer" => new[] 
            { 
                Permissions.Read 
            },
            // Handle role names with different casing
            _ when role.Equals("Manager", StringComparison.OrdinalIgnoreCase) => new[] 
            { 
                Permissions.Read, 
                Permissions.Create, 
                Permissions.Update, 
                Permissions.Delete 
            },
            _ when role.Equals("StoreCustomer", StringComparison.OrdinalIgnoreCase) => new[] 
            { 
                Permissions.Read 
            },
            _ => Array.Empty<string>()
        };
    }
}
