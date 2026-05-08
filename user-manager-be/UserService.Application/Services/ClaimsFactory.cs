using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using UserService.Application.Constants;
using UserService.Application.Interfaces.Services;
using UserService.Domain.Constants;

namespace UserService.Application.Services;

public class ClaimsFactory : IClaimsFactory
{
    public IReadOnlyList<Claim> CreateAccessTokenClaims(
        Guid userId,
        string email,
        string? displayName,
        IEnumerable<string> roles)
    {
        return CreateAccessTokenClaims(userId, email, displayName, roles, Enumerable.Empty<string>());
    }

    public IReadOnlyList<Claim> CreateAccessTokenClaims(
        Guid userId,
        string email,
        string? displayName,
        IEnumerable<string> roles,
        IEnumerable<string> permissions)
    {
        return CreateAccessTokenClaims(userId, email, displayName, roles, permissions, null, null);
    }

    public IReadOnlyList<Claim> CreateAccessTokenClaims(
        Guid userId,
        string email,
        string? displayName,
        IEnumerable<string> roles,
        IEnumerable<string> permissions,
        Guid? applicationId,
        string? scope = null,
        IEnumerable<Guid>? ssoAppIds = null,
        IEnumerable<string>? oauthScopes = null)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(JwtClaimNames.UserId, userId.ToString()),
            new(JwtRegisteredClaimNames.Email, email),
            new(ClaimTypes.Name, displayName ?? IdentityDefaults.FallbackDisplayName),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var permissionsList = permissions as IList<string> ?? permissions.ToList();
        if (permissionsList.Count > 0)
        {
            var permissionsValue = string.Join(",", permissionsList.Distinct(StringComparer.OrdinalIgnoreCase));
            claims.Add(new Claim(JwtClaimNames.Permissions, permissionsValue));
        }
        if (applicationId.HasValue)
        {
            claims.Add(new Claim(JwtClaimNames.ApplicationId, applicationId.Value.ToString()));
        }
        var combinedScopes = new List<string>();
        if (!string.IsNullOrWhiteSpace(scope)) combinedScopes.Add(scope);
        if (oauthScopes != null) combinedScopes.AddRange(oauthScopes);

        if (combinedScopes.Count > 0)
        {
            var value = string.Join(" ", combinedScopes
                .Select(s => (s ?? string.Empty).Trim())
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Distinct(StringComparer.OrdinalIgnoreCase));
            
            if (!string.IsNullOrWhiteSpace(value))
                claims.Add(new Claim(JwtClaimNames.Scope, value));
        }

        if (ssoAppIds != null)
        {
            var ids = ssoAppIds as IList<Guid> ?? ssoAppIds.ToList();
            if (ids.Count > 0)
                claims.Add(new Claim(JwtClaimNames.SsoAppIds, string.Join(",", ids)));
        }
        return claims;
    }
}
