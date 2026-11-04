namespace UserService.Application.Configuration;

public class JwtConfiguration
{
    /// <summary>Legacy HMAC secret. No longer used to sign tokens (RS256 key ring is used); kept for configuration compatibility.</summary>
    public string SecretKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int AccessTokenExpirationMinutes { get; set; } = 15;
    public int RefreshTokenExpirationDays { get; set; } = 7;
}
