using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace CartService.API.Middleware;

/// <summary>
/// Middleware to log identity access token details for all incoming requests.
/// Logs user information, roles, permissions, and token metadata.
/// </summary>
public class TokenLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TokenLoggingMiddleware> _logger;

    public TokenLoggingMiddleware(RequestDelegate next, ILogger<TokenLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Check if Authorization header exists
        if (context.Request.Headers.TryGetValue("Authorization", out var authHeader))
        {
            var token = authHeader.ToString();

            if (token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                // Extract the token (remove "Bearer " prefix)
                var jwtToken = token.Substring("Bearer ".Length).Trim();

                try
                {
                    // Decode the JWT token
                    var handler = new JwtSecurityTokenHandler();
                    var jsonToken = handler.ReadToken(jwtToken) as JwtSecurityToken;

                    if (jsonToken != null)
                    {
                        // Extract claims
                        var username = jsonToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name || c.Type == "name")?.Value;
                        var userId = jsonToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier || c.Type == "sub")?.Value;
                        var email = jsonToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email || c.Type == "email")?.Value;
                        var roles = jsonToken.Claims.Where(c => c.Type == "roles" || c.Type == ClaimTypes.Role).Select(c => c.Value).ToList();
                        var permissions = jsonToken.Claims.Where(c => c.Type == "permission").Select(c => c.Value).ToList();
                        var issuer = jsonToken.Issuer;
                        var audience = jsonToken.Audiences.FirstOrDefault();
                        var expirationTime = jsonToken.ValidTo;
                        var issuedAt = jsonToken.IssuedAt;

                        // Log comprehensive token details
                        _logger.LogInformation(
                            "=== Token Details === " +
                            "Endpoint: {Method} {Path}, " +
                            "User: {Username}, " +
                            "UserId: {UserId}, " +
                            "Email: {Email}, " +
                            "Roles: [{Roles}], " +
                            "Permissions: [{Permissions}], " +
                            "Issuer: {Issuer}, " +
                            "Audience: {Audience}, " +
                            "Issued At: {IssuedAt}, " +
                            "Expires: {Expiration}, " +
                            "IP Address: {IpAddress}",
                            context.Request.Method,
                            context.Request.Path,
                            username ?? "Unknown",
                            userId ?? "Unknown",
                            email ?? "Unknown",
                            string.Join(", ", roles),
                            string.Join(", ", permissions),
                            issuer,
                            audience ?? "Unknown",
                            issuedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                            expirationTime.ToString("yyyy-MM-dd HH:mm:ss"),
                            context.Connection.RemoteIpAddress?.ToString() ?? "Unknown"
                        );

                        // Add user information to HttpContext for downstream use
                        context.Items["Username"] = username;
                        context.Items["UserId"] = userId;
                        context.Items["Roles"] = roles;
                        context.Items["Permissions"] = permissions;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to decode JWT token for logging. Token might be invalid or malformed.");
                }
            }
        }
        else
        {
            // Log requests without authorization header
            _logger.LogWarning(
                "Request without Authorization header: {Method} {Path} from {IpAddress}",
                context.Request.Method,
                context.Request.Path,
                context.Connection.RemoteIpAddress?.ToString() ?? "Unknown"
            );
        }

        // Call the next middleware in the pipeline
        await _next(context);

        // Optionally log response status
        _logger.LogInformation(
            "Response: {Method} {Path} returned {StatusCode}",
            context.Request.Method,
            context.Request.Path,
            context.Response.StatusCode
        );
    }
}
