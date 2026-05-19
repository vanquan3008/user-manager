using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using UserService.Application.Constants;
using UserService.Application.DTOs.Auth;
using UserService.Application.Interfaces.Services;
using UserService.Domain.Constants;

namespace UserService.Application.Services;

public class ClaimsFactory : IClaimsFactory
{
    public IReadOnlyList<Claim> CreateAccessTokenClaims(AccessTokenRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, request.UserId.ToString()),
            new(ClaimTypes.NameIdentifier, request.UserId.ToString()),
            new(JwtClaimNames.UserId, request.UserId.ToString()),
            new(JwtRegisteredClaimNames.Email, request.Email),
            new(ClaimTypes.Name, request.DisplayName ?? IdentityDefaults.FallbackDisplayName),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };

        var roles = request.Roles ?? Enumerable.Empty<string>();
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var permissionsList = request.Permissions?.ToList() ?? new List<string>();
        if (permissionsList.Count > 0)
        {
            var permissionsValue = string.Join(",", permissionsList.Distinct(StringComparer.OrdinalIgnoreCase));
            claims.Add(new Claim(JwtClaimNames.Permissions, permissionsValue));
        }

        if (request.ApplicationId.HasValue)
        {
            claims.Add(new Claim(JwtClaimNames.ApplicationId, request.ApplicationId.Value.ToString()));
        }

        var combinedScopes = new List<string>();
        if (!string.IsNullOrWhiteSpace(request.Scope)) combinedScopes.Add(request.Scope);
        if (request.OAuthScopes != null) combinedScopes.AddRange(request.OAuthScopes);

        if (combinedScopes.Count > 0)
        {
            var value = string.Join(" ", combinedScopes
                .Select(s => (s ?? string.Empty).Trim())
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Distinct(StringComparer.OrdinalIgnoreCase));

            if (!string.IsNullOrWhiteSpace(value))
                claims.Add(new Claim(JwtClaimNames.Scope, value));
        }

        if (request.SSOAppIds != null)
        {
            var ids = request.SSOAppIds as IList<Guid> ?? request.SSOAppIds.ToList();
            if (ids.Count > 0)
                claims.Add(new Claim(JwtClaimNames.SSOAppIds, string.Join(",", ids)));
        }

        return claims;
    }
}
