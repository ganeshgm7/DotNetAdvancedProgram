using IdentityService.API.DTOs;
using IdentityService.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdentityService.API.Controllers;

/// <summary>
/// Authentication controller for token management operations.
/// Provides endpoints for generating, validating, refreshing, and revoking JWT tokens.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ITokenService _tokenService;
    private readonly ILogger<AuthController> _logger;

    /// <summary>
    /// Initializes a new instance of the AuthController.
    /// </summary>
    /// <param name="tokenService">The token service for authentication operations.</param>
    /// <param name="logger">Logger for controller operations.</param>
    public AuthController(ITokenService tokenService, ILogger<AuthController> logger)
    {
        _tokenService = tokenService;
        _logger = logger;
    }

    /// <summary>
    /// Authenticates a user and generates JWT access and refresh tokens.
    /// </summary>
    /// <param name="request">Login credentials containing username and password.</param>
    /// <returns>
    /// Returns <see cref="OkObjectResult"/> with token response including access token, refresh token, and user details;
    /// Returns <see cref="UnauthorizedResult"/> if authentication fails.
    /// </returns>
    /// <response code="200">Successfully authenticated and tokens generated.</response>
    /// <response code="401">Authentication failed - invalid credentials.</response>
    /// <response code="500">Internal server error during token generation.</response>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new { message = "Username and password are required." });
            }

            _logger.LogInformation("Login attempt for user: {Username}", request.Username);

            // In Azure AD scenario, the username is typically the UPN (User Principal Name)
            // Password validation is done by Azure AD, not by this service
            // This endpoint serves as a wrapper to generate tokens for authenticated users
            TokenResponse response = await _tokenService.GenerateTokenAsync(request.Username);

            _logger.LogInformation("Login successful for user: {Username}", request.Username);

            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Login failed for user: {Username}", request.Username);
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for user: {Username}", request.Username);
            return StatusCode(500, new { message = "An error occurred during authentication." });
        }
    }

    /// <summary>
    /// Validates a JWT token and returns the token's claims and status.
    /// </summary>
    /// <param name="request">Token validation request containing the JWT token to validate.</param>
    /// <returns>
    /// Returns <see cref="OkObjectResult"/> with validation response indicating if token is valid and user details;
    /// Returns <see cref="BadRequestResult"/> if token validation fails.
    /// </returns>
    /// <response code="200">Token validation completed (check IsValid property in response).</response>
    /// <response code="400">Invalid request or malformed token.</response>
    [HttpPost("validate")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(TokenValidationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ValidateToken([FromBody] TokenValidationRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Token))
            {
                return BadRequest(new { message = "Token is required." });
            }

            _logger.LogInformation("Token validation requested");

            TokenValidationResponse response = await _tokenService.ValidateTokenAsync(request.Token);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Token validation failed");
            return Ok(new TokenValidationResponse
            {
                IsValid = false,
                Message = "Token validation failed: " + ex.Message
            });
        }
    }

    /// <summary>
    /// Refreshes an expired or soon-to-expire access token using a valid refresh token.
    /// Note: This endpoint is designed for Azure AD scenarios where refresh is managed client-side.
    /// </summary>
    /// <param name="request">Refresh token request containing the current access token and refresh token.</param>
    /// <returns>
    /// Returns <see cref="OkObjectResult"/> with new token response;
    /// Returns <see cref="UnauthorizedResult"/> if refresh token is invalid.
    /// </returns>
    /// <response code="200">Token successfully refreshed.</response>
    /// <response code="401">Refresh token is invalid or expired.</response>
    /// <response code="501">Refresh token flow not implemented (use Azure AD MSAL client libraries).</response>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status501NotImplemented)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.AccessToken) || string.IsNullOrWhiteSpace(request.RefreshToken))
            {
                return BadRequest(new { message = "Access token and refresh token are required." });
            }

            _logger.LogInformation("Token refresh requested");

            TokenResponse response = await _tokenService.RefreshTokenAsync(request);

            return Ok(response);
        }
        catch (NotImplementedException ex)
        {
            _logger.LogWarning("Refresh token not implemented - client should use Azure AD MSAL");
            return StatusCode(501, new
            {
                message = ex.Message,
                guidance = "Use Microsoft Authentication Library (MSAL) on the client side to handle token refresh with Azure AD.",
                documentation = "https://learn.microsoft.com/en-us/azure/active-directory/develop/msal-overview"
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Invalid refresh token");
            return Unauthorized(new { message = "Invalid or expired refresh token." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during token refresh");
            return StatusCode(500, new { message = "An error occurred during token refresh." });
        }
    }

    /// <summary>
    /// Revokes all tokens for a specific user.
    /// Note: Actual token revocation in Azure AD requires Graph API permissions and admin consent.
    /// </summary>
    /// <param name="username">The username whose tokens should be revoked.</param>
    /// <returns>
    /// Returns <see cref="OkResult"/> indicating revocation request was processed.
    /// </returns>
    /// <response code="200">Token revocation request processed.</response>
    /// <response code="400">Invalid username provided.</response>
    /// <response code="401">Unauthorized - requires authentication.</response>
    [HttpPost("revoke/{username}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RevokeToken(string username)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return BadRequest(new { message = "Username is required." });
            }

            _logger.LogInformation("Token revocation requested for user: {Username}", username);

            await _tokenService.RevokeTokenAsync(username);

            return Ok(new
            {
                message = $"Token revocation processed for user: {username}",
                note = "Complete token revocation requires Azure AD Graph API access. Please use Azure Portal or Graph API for full revocation."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during token revocation for user: {Username}", username);
            return StatusCode(500, new { message = "An error occurred during token revocation." });
        }
    }

    /// <summary>
    /// Registers a new user in the system.
    /// Note: In Azure AD scenarios, user registration is typically handled through Azure Portal or Graph API.
    /// </summary>
    /// <param name="request">Registration request containing user details.</param>
    /// <returns>
    /// Returns <see cref="StatusCodeResult"/> indicating that registration should be done via Azure AD.
    /// </returns>
    /// <response code="501">User registration not implemented - use Azure AD user management.</response>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status501NotImplemented)]
    public IActionResult Register([FromBody] RegisterRequest request)
    {
        _logger.LogInformation("Registration attempt for user: {Username}", request.Username);

        return StatusCode(501, new
        {
            message = "User registration is managed through Azure AD.",
            guidance = "Please use Azure Portal or Microsoft Graph API to create users and assign roles.",
            documentation = "https://learn.microsoft.com/en-us/graph/api/user-post-users"
        });
    }

    /// <summary>
    /// Gets information about the currently authenticated user.
    /// </summary>
    /// <returns>
    /// Returns <see cref="OkObjectResult"/> with current user information.
    /// </returns>
    /// <response code="200">User information retrieved successfully.</response>
    /// <response code="401">Unauthorized - authentication required.</response>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult GetCurrentUser()
    {
        var userInfo = new
        {
            username = User.Identity?.Name,
            claims = User.Claims.Select(c => new { c.Type, c.Value }),
            roles = User.Claims.Where(c => c.Type == "roles" || c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role")
                        .Select(c => c.Value),
            permissions = User.Claims.Where(c => c.Type == "permission")
                            .Select(c => c.Value)
        };

        return Ok(userInfo);
    }
}
