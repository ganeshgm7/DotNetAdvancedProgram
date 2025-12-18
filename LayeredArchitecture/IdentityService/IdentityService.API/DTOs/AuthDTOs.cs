namespace IdentityService.API.DTOs;

public class RegisterRequest
{
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string Role { get; set; } = null!;
}

public class LoginRequest
{
    public string Username { get; set; } = null!;
    public string Password { get; set; } = null!;
}

public class TokenResponse
{
    /// <summary>
    /// ID Token (OIDC) - Contains user identity information.
    /// Use this for authentication and displaying user info.
    /// </summary>
    public string IdToken { get; set; } = null!;
    
    /// <summary>
    /// Access Token (OAuth 2.0) - Contains authorization claims.
    /// Use this to access protected APIs.
    /// </summary>
    public string AccessToken { get; set; } = null!;
    
    /// <summary>
    /// Refresh Token - Use to get new tokens without re-authenticating.
    /// </summary>
    public string RefreshToken { get; set; } = null!;
    
    /// <summary>
    /// Token type - Always "Bearer" for JWT tokens.
    /// </summary>
    public string TokenType { get; set; } = "Bearer";
    
    public DateTime IdTokenExpiry { get; set; }
    public DateTime AccessTokenExpiry { get; set; }
    public DateTime RefreshTokenExpiry { get; set; }
    
    /// <summary>
    /// User information returned with the token.
    /// </summary>
    public string Username { get; set; } = null!;
    public IEnumerable<string> Roles { get; set; } = null!;
    public IEnumerable<string> Permissions { get; set; } = null!;
}

public class RefreshTokenRequest
{
    public string AccessToken { get; set; } = null!;
    public string RefreshToken { get; set; } = null!;
}

public class TokenValidationRequest
{
    public string Token { get; set; } = null!;
}

public class TokenValidationResponse
{
    public bool IsValid { get; set; }
    public string? Username { get; set; }
    public IEnumerable<string>? Roles { get; set; }
    public IEnumerable<string>? Permissions { get; set; }
    public string? Message { get; set; }
}
