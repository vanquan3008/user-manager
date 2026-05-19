using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using UserService.Application.Configuration;
using UserService.Application.DTOs.Auth;
using UserService.Application.Interfaces.Services;
using UserService.Domain.Exceptions;

namespace UserService.Application.Services;

public class TokenService : ITokenService
{
    private readonly IClaimsFactory _claimsFactory;
    private readonly AppSettings _appSettings;

    public TokenService(IClaimsFactory claimsFactory, AppSettings appSettings)
    {
        _claimsFactory = claimsFactory;
        _appSettings = appSettings;
    }

    public string GenerateAccessToken(AccessTokenRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var claims = _claimsFactory.CreateAccessTokenClaims(request);

        var jwt = _appSettings.Jwt;
        if (string.IsNullOrWhiteSpace(jwt.SecretKey))
            throw new InvalidOperationException("JWT SecretKey is missing.");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SecretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: jwt.Issuer,
            audience: jwt.Audience,
            claims: claims.ToList(),
            expires: DateTime.UtcNow.AddMinutes(jwt.AccessTokenExpirationMinutes),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    public bool ValidateToken(string token)
    {
        try
        {
            var jwt = _appSettings.Jwt;
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(jwt.SecretKey);

            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwt.Issuer,
                ValidAudience = jwt.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ClockSkew = TimeSpan.Zero
            }, out _);

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
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);
            
            var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub);
            
            if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
            {
                return userId;
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    public string GenerateLogoutToken(Guid userId, string sessionId)
    {
        var jwt = _appSettings.Jwt;
        if (string.IsNullOrWhiteSpace(jwt.SecretKey))
            throw new InvalidOperationException("JWT SecretKey is missing.");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SecretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
            new("sid", sessionId),
            new("events", "{\"http://schemas.openid.net/event/backchannel-logout\":{}}", ClaimValueTypes.String)
        };

        var token = new JwtSecurityToken(
            issuer: jwt.Issuer,
            audience: jwt.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(5), 
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
