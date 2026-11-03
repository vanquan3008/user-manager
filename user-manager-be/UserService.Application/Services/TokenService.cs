using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using UserService.Application.Common;
using UserService.Application.Configuration;
using UserService.Application.DTOs.Auth;
using UserService.Application.Interfaces.Services;
using UserService.Application.Interfaces.Services.Security;
using UserService.Domain.Interfaces;

namespace UserService.Application.Services;

public class TokenService : ITokenService
{
    private readonly IClaimsFactory _claimsFactory;
    private readonly AppSettings _appSettings;
    private readonly ISigningKeyProvider _keys;
    private readonly IClock _clock;

    public TokenService(IClaimsFactory claimsFactory, AppSettings appSettings, ISigningKeyProvider keys, IClock clock)
    {
        _claimsFactory = claimsFactory;
        _appSettings = appSettings;
        _keys = keys;
        _clock = clock;
    }

    public string GenerateAccessToken(AccessTokenRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        var claims = _claimsFactory.CreateAccessTokenClaims(request);
        var now = _clock.UtcNow;

        return Write(new SecurityTokenDescriptor
        {
            Issuer = _appSettings.Jwt.Issuer,
            Audience = _appSettings.Jwt.Audience,
            Subject = new ClaimsIdentity(claims),
            IssuedAt = now,
            NotBefore = now,
            Expires = now.AddMinutes(_appSettings.Jwt.AccessTokenExpirationMinutes),
            SigningCredentials = _keys.GetSigningCredentials(),
            TokenType = "at+jwt"
        });
    }

    public string GenerateIdToken(IdTokenRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        var now = _clock.UtcNow;
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, request.UserId.ToString()),
            new(JwtRegisteredClaimNames.AuthTime, ToUnix(request.AuthTime).ToString(), ClaimValueTypes.Integer64),
            new(JwtRegisteredClaimNames.Azp, request.ClientId)
        };

        if (!string.IsNullOrWhiteSpace(request.Nonce))
            claims.Add(new Claim(JwtRegisteredClaimNames.Nonce, request.Nonce));
        if (request.SessionId.HasValue)
            claims.Add(new Claim("sid", request.SessionId.Value.ToString()));
        if (!string.IsNullOrWhiteSpace(request.AuthenticationMethod))
            claims.Add(new Claim("amr", request.AuthenticationMethod));

        var scopes = new HashSet<string>(request.Scopes, StringComparer.OrdinalIgnoreCase);
        if (scopes.Contains("email") && !string.IsNullOrWhiteSpace(request.Email))
        {
            claims.Add(new Claim(JwtRegisteredClaimNames.Email, request.Email));
            claims.Add(new Claim("email_verified", request.EmailVerified ? "true" : "false", ClaimValueTypes.Boolean));
        }
        if (scopes.Contains("profile"))
        {
            if (!string.IsNullOrWhiteSpace(request.Name))
                claims.Add(new Claim(JwtRegisteredClaimNames.Name, request.Name));
            if (!string.IsNullOrWhiteSpace(request.PreferredUsername))
                claims.Add(new Claim("preferred_username", request.PreferredUsername));
        }

        return Write(new SecurityTokenDescriptor
        {
            Issuer = _appSettings.Jwt.Issuer,
            Audience = request.ClientId,
            Subject = new ClaimsIdentity(claims),
            IssuedAt = now,
            NotBefore = now,
            Expires = now.AddMinutes(_appSettings.Jwt.AccessTokenExpirationMinutes),
            SigningCredentials = _keys.GetSigningCredentials()
        });
    }

    public string GenerateRefreshToken() => SecureTokens.Create();

    public bool ValidateToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return false;
        try
        {
            new JwtSecurityTokenHandler { MapInboundClaims = false }.ValidateToken(token, CreateValidationParameters(), out _);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public Guid? GetUserIdFromToken(string token)
    {
        try
        {
            var jwtToken = new JwtSecurityTokenHandler().ReadJwtToken(token);
            var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub);
            return userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId) ? userId : null;
        }
        catch
        {
            return null;
        }
    }

    public string GenerateLogoutToken(Guid userId, string sessionId, string audience)
    {
        var now = _clock.UtcNow;
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new("sid", sessionId)
        };

        return Write(new SecurityTokenDescriptor
        {
            Issuer = _appSettings.Jwt.Issuer,
            Audience = audience,
            Subject = new ClaimsIdentity(claims),
            IssuedAt = now,
            Expires = now.AddMinutes(2),
            Claims = new Dictionary<string, object>
            {
                ["events"] = new Dictionary<string, object>
                {
                    ["http://schemas.openid.net/event/backchannel-logout"] = new Dictionary<string, object>()
                }
            },
            SigningCredentials = _keys.GetSigningCredentials(),
            TokenType = "logout+jwt"
        });
    }

    public TokenValidationParameters CreateValidationParameters() => new()
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = _appSettings.Jwt.Issuer,
        ValidAudience = _appSettings.Jwt.Audience,
        IssuerSigningKeys = _keys.GetValidationKeys(),
        ValidAlgorithms = new[] { SecurityAlgorithms.RsaSha256 },
        ClockSkew = TimeSpan.FromSeconds(30)
    };

    private static string Write(SecurityTokenDescriptor descriptor)
    {
        var handler = new JwtSecurityTokenHandler { SetDefaultTimesOnTokenCreation = false };
        return handler.WriteToken(handler.CreateJwtSecurityToken(descriptor));
    }

    private static long ToUnix(DateTime utc) => new DateTimeOffset(DateTime.SpecifyKind(utc, DateTimeKind.Utc)).ToUnixTimeSeconds();
}
