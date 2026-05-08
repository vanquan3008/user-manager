namespace UserService.Application.Interfaces.Services;

public interface ITokenService
{
    string GenerateAccessToken(Guid userId, string email, string? displayName, IEnumerable<string> roles);
    string GenerateAccessToken(Guid userId, string email, string? displayName, IEnumerable<string> roles, IEnumerable<string> permissions);
    string GenerateAccessToken(Guid userId, string email, string? displayName, IEnumerable<string> roles, IEnumerable<string> permissions, Guid? applicationId, string? scope = null, IEnumerable<Guid>? ssoAppIds = null, IEnumerable<string>? oauthScopes = null);
    string GenerateRefreshToken();
    Boolean ValidateToken(string token);
    Guid? GetUserIdFromToken(string token);
    string GenerateLogoutToken(Guid userId, string sessionId);
}
