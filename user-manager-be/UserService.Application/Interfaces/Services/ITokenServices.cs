using UserService.Application.DTOs.Auth;

namespace UserService.Application.Interfaces.Services;

public interface ITokenService
{
    /// <summary>Signs an RS256 access token with the active key of the signing key ring.</summary>
    string GenerateAccessToken(AccessTokenRequest request);
    string GenerateIdToken(IdTokenRequest request);
    /// <summary>Opaque, URL-safe, 256-bit random refresh token.</summary>
    string GenerateRefreshToken();
    bool ValidateToken(string token);
    Guid? GetUserIdFromToken(string token);
    /// <summary>OIDC Back-Channel Logout 1.0 logout token for <paramref name="audience"/> (the client id).</summary>
    string GenerateLogoutToken(Guid userId, string sessionId, string audience);
}
