using Azure.Core;
using Azure.Identity;
using IdentityService.API.Configuration;
using IdentityService.API.DTOs;
using IdentityService.API.Models;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace IdentityService.API.Services;

/// <summary>
/// Azure AD Token Service - Integrates with Azure Entra ID (Azure AD) for authentication
/// and generates JWT tokens for API authorization.
/// </summary>
public class AzureAdTokenService : ITokenService
{
    private readonly AzureAdSettings _azureAdSettings;
    private readonly JwtSettings _jwtSettings;
    private readonly ILogger<AzureAdTokenService> _logger;
    private readonly IConfidentialClientApplication _confidentialClient;

    public AzureAdTokenService(
        IOptions<AzureAdSettings> azureAdSettings,
        IOptions<JwtSettings> jwtSettings,
        ILogger<AzureAdTokenService> logger)
    {
        _azureAdSettings = azureAdSettings.Value;
        _jwtSettings = jwtSettings.Value;
        _logger = logger;

        _confidentialClient = ConfidentialClientApplicationBuilder
            .Create(_azureAdSettings.ClientId)
            .WithClientSecret(_azureAdSettings.ClientSecret)
            .WithAuthority(new Uri($"{_azureAdSettings.Instance}{_azureAdSettings.TenantId}"))
            .Build();
    }

    public async Task<TokenResponse> GenerateTokenAsync(string username)
    {
        try
        {
            // Step 1: Get Graph API token to query user info
            string[] scopes = ["https://graph.microsoft.com/.default"];

            AuthenticationResult graphAuthResult = await _confidentialClient
                .AcquireTokenForClient(scopes)
                .ExecuteAsync();

            // Step 2: Get user information from Azure AD via Graph API
            GraphServiceClient graphClient = CreateGraphClient(graphAuthResult.AccessToken);

            User? user = await graphClient.Users[username].GetAsync() ?? throw new InvalidOperationException($"User {username} not found in Azure AD");

            // Step 3: Get user's app roles from Azure AD
            IEnumerable<string> roles = await GetUserRolesAsync(graphClient, user.Id!);
            IEnumerable<string> permissions = roles.SelectMany(GetPermissionsForRole).Distinct();

            DateTime now = DateTime.UtcNow;
            DateTime idTokenExpiry = now.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes);
            DateTime accessTokenExpiry = now.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes);

            // Step 4: Generate ID Token (OpenID Connect) for authentication
            string idToken = GenerateIdToken(username, user, idTokenExpiry, roles);

            // Step 5: Generate Access Token (OAuth 2.0) for API authorization
            string accessToken = GenerateAccessToken(username, user, accessTokenExpiry, roles, permissions);

            // Step 6: Generate Refresh Token
            string refreshToken = GenerateRefreshToken();

            _logger.LogInformation("Generated tokens for user {Username} via Azure AD with roles: {Roles}", username, string.Join(", ", roles));

            return new TokenResponse
            {
                IdToken = idToken,
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                TokenType = "Bearer",
                IdTokenExpiry = idTokenExpiry,
                AccessTokenExpiry = accessTokenExpiry,
                RefreshTokenExpiry = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays),
                Username = user.UserPrincipalName ?? username,
                Roles = roles,
                Permissions = permissions
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate token for user {Username}", username);
            throw;
        }
    }

    /// <summary>
    /// Generates an ID Token (OpenID Connect) from Azure AD user information.
    /// </summary>
    private string GenerateIdToken(string username, Microsoft.Graph.Models.User user, DateTime expiry, IEnumerable<string> roles)
    {
        List<Claim> claims =
        [
            // Standard OIDC claims
            new(JwtRegisteredClaimNames.Sub, user.Id ?? username),
            new(JwtRegisteredClaimNames.Email, user.Mail ?? user.UserPrincipalName ?? username),
            new(JwtRegisteredClaimNames.Name, user.DisplayName ?? username),
            new("preferred_username", user.UserPrincipalName ?? username),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
            
            // Azure AD claims
            new("tid", _azureAdSettings.TenantId),
            new("oid", user.Id ?? Guid.NewGuid().ToString()),
            new("upn", user.UserPrincipalName ?? username)
        ];

        // Add roles
        foreach (var role in roles)
        {
            claims.Add(new Claim("role", role));
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        SymmetricSecurityKey key = new(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
        SigningCredentials creds = new(key, SecurityAlgorithms.HmacSha256);

        SecurityTokenDescriptor tokenDescriptor = new()
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expiry,
            Issuer = _jwtSettings.Issuer,
            Audience = _azureAdSettings.ClientId, // ID Token audience is the client
            SigningCredentials = creds
        };

        JwtSecurityTokenHandler tokenHandler = new();
        SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    /// <summary>
    /// Generates an Access Token (OAuth 2.0) for API authorization.
    /// </summary>
    private string GenerateAccessToken(
        string username,
        User user,
        DateTime expiry,
        IEnumerable<string> roles,
        IEnumerable<string> permissions)
    {
        var claims = new List<Claim>
        {
            // Identity claims
            new(ClaimTypes.NameIdentifier, user.Id ?? username),
            new(ClaimTypes.Name, user.UserPrincipalName ?? username),
            new(ClaimTypes.Email, user.Mail ?? user.UserPrincipalName ?? username),
            
            // Authorization claims
            new("scope", "api.access"),
            
            // Azure AD claims
            new("tid", _azureAdSettings.TenantId),
            new("oid", user.Id ?? Guid.NewGuid().ToString()),
            new("azp", _azureAdSettings.ClientId),
            new("ver", "2.0"),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        // Add roles
        foreach (var role in roles)
        {
            claims.Add(new Claim("roles", role));
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        // Add permissions
        foreach (var permission in permissions)
        {
            claims.Add(new Claim("permission", permission));
        }

        SymmetricSecurityKey key = new(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
        SigningCredentials creds = new(key, SecurityAlgorithms.HmacSha256);

        SecurityTokenDescriptor tokenDescriptor = new()
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expiry,
            Issuer = _jwtSettings.Issuer,
            Audience = _jwtSettings.Audience, // Access Token audience is the API
            SigningCredentials = creds
        };

        JwtSecurityTokenHandler tokenHandler = new();
        SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public async Task<TokenResponse> RefreshTokenAsync(RefreshTokenRequest request)
    {
        try
        {
            // Decode the access token to get the username
            JwtSecurityTokenHandler handler = new();
            JwtSecurityToken? jsonToken = handler.ReadToken(request.AccessToken) as JwtSecurityToken;
            string? username = jsonToken?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name || c.Type == "upn")?.Value;

            if (string.IsNullOrEmpty(username))
            {
                throw new UnauthorizedAccessException("Invalid access token");
            }

            _logger.LogInformation("Refreshing tokens for Azure AD user {Username}", username);

            // Generate new tokens
            return await GenerateTokenAsync(username);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to refresh token");
            throw new UnauthorizedAccessException("Token refresh failed", ex);
        }
    }

    public async Task<TokenValidationResponse> ValidateTokenAsync(string token)
    {
        try
        {
            SymmetricSecurityKey key = new(Encoding.UTF8.GetBytes(_jwtSettings.Secret));

            TokenValidationParameters validationParameters = new()
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _jwtSettings.Issuer,
                ValidAudience = _jwtSettings.Audience,
                IssuerSigningKey = key,
                ClockSkew = TimeSpan.Zero
            };

            JwtSecurityTokenHandler handler = new();
            ClaimsPrincipal principal = handler.ValidateToken(token, validationParameters, out _);

            string? username = principal.FindFirst(ClaimTypes.Name)?.Value;
            IEnumerable<string> roles = principal.FindAll("roles").Select(c => c.Value);
            IEnumerable<string> permissions = principal.FindAll("permission").Select(c => c.Value);

            return new TokenValidationResponse
            {
                IsValid = true,
                Username = username,
                Roles = roles,
                Permissions = permissions,
                Message = "Token is valid"
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Token validation failed");
            return new TokenValidationResponse
            {
                IsValid = false,
                Message = $"Token validation failed: {ex.Message}"
            };
        }
    }

    public Task RevokeTokenAsync(string username)
    {
        _logger.LogInformation("Token revocation for {Username} - Azure AD tokens should be revoked via Azure portal or Graph API", username);
        // In production, you would:
        // 1. Revoke refresh tokens via Graph API
        // 2. Add access token to blacklist
        // 3. Update Conditional Access policies
        return Task.CompletedTask;
    }

    private static GraphServiceClient CreateGraphClient(string accessToken)
    {
        TokenCredential credential = new AccessTokenCredential(accessToken);
        return new GraphServiceClient(credential);
    }

    private async Task<IEnumerable<string>> GetUserRolesAsync(GraphServiceClient graphClient, string userId)
    {
        try
        {
            // Get app role assignments for the user
            AppRoleAssignmentCollectionResponse? appRoleAssignments = await graphClient.Users[userId]
                .AppRoleAssignments
                .GetAsync();

            List<string> roles = [];

            if (appRoleAssignments?.Value != null)
            {
                foreach (AppRoleAssignment assignment in appRoleAssignments.Value)
                {
                    string? roleName = MapAppRoleIdToName(assignment.AppRoleId);
                    if (!string.IsNullOrEmpty(roleName))
                    {
                        roles.Add(roleName);
                    }
                }
            }

            // Default to StoreCustomer if no roles assigned
            return roles.Any() ? roles : new[] { AppRoles.StoreCustomer };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get user roles from Azure AD, defaulting to StoreCustomer");
            return new[] { AppRoles.StoreCustomer };
        }
    }

    private string? MapAppRoleIdToName(Guid? appRoleId)
    {
        if (appRoleId == null) return null;

        string roleIdLower = appRoleId.ToString()!.ToLower();

        if (roleIdLower.Equals(_azureAdSettings.RoleMappings.ManagerRoleId, StringComparison.OrdinalIgnoreCase))
        {
            return AppRoles.Manager;
        }

        if (roleIdLower.Equals(_azureAdSettings.RoleMappings.StoreCustomerRoleId.ToLower(), StringComparison.OrdinalIgnoreCase))
        {
            return AppRoles.StoreCustomer;
        }

        return null;
    }

    private static IEnumerable<string> GetPermissionsForRole(string role)
    {
        return role switch
        {
            AppRoles.Manager => [AppPermissions.Read, AppPermissions.Create, AppPermissions.Update, AppPermissions.Delete],
            AppRoles.StoreCustomer => [AppPermissions.Read],
            _ => Array.Empty<string>()
        };
    }

    private static string GenerateRefreshToken()
    {
        byte[] randomBytes = new byte[64];
        using RandomNumberGenerator rng = System.Security.Cryptography.RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    // Helper class for token credential
    private sealed class AccessTokenCredential(string accessToken) : TokenCredential
    {
        private readonly string _accessToken = accessToken;

        public override AccessToken GetToken(TokenRequestContext requestContext, CancellationToken cancellationToken)
        {
            return new AccessToken(_accessToken, DateTimeOffset.UtcNow.AddHours(1));
        }

        public override ValueTask<AccessToken> GetTokenAsync(TokenRequestContext requestContext, CancellationToken cancellationToken)
        {
            return new ValueTask<AccessToken>(new AccessToken(_accessToken, DateTimeOffset.UtcNow.AddHours(1)));
        }
    }
}
