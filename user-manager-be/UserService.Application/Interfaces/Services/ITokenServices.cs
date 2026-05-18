using UserService.Application.DTOs.Auth;

namespace UserService.Application.Interfaces.Services;

public interface ITokenService
{
    string GenerateAccessToken(AccessTokenRequest request);
    string GenerateRefreshToken();
    bool ValidateToken(string token);
    Guid? GetUserIdFromToken(string token);
    string GenerateLogoutToken(Guid userId, string sessionId);
}
