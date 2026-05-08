using System.Security.Claims;

namespace UserService.Application.Interfaces.Services;
public interface IClaimsFactory
{
    IReadOnlyList<Claim> CreateAccessTokenClaims(
        Guid userId,
        string email,
        string? displayName,
        IEnumerable<string> roles);

    IReadOnlyList<Claim> CreateAccessTokenClaims(
        Guid userId,
        string email,
        string? displayName,
        IEnumerable<string> roles,
        IEnumerable<string> permissions);

    IReadOnlyList<Claim> CreateAccessTokenClaims(
        Guid userId,
        string email,
        string? displayName,
        IEnumerable<string> roles,
        IEnumerable<string> permissions,
        Guid? applicationId,
        string? scope = null,
        IEnumerable<Guid>? ssoAppIds = null,
        IEnumerable<string>? oauthScopes = null);
}
