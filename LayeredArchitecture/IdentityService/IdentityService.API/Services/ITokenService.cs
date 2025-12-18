using IdentityService.API.DTOs;

namespace IdentityService.API.Services;

public interface ITokenService
{
    Task<TokenResponse> GenerateTokenAsync(string username);
    Task<TokenResponse> RefreshTokenAsync(RefreshTokenRequest request);
    Task<TokenValidationResponse> ValidateTokenAsync(string token);
    Task RevokeTokenAsync(string username);
}
