namespace UserService.Application.Configuration;

public class RateLimitConfiguration
{
    /// <summary>Global per-client fixed-window limit.</summary>
    public int RequestLimit { get; set; } = 100;
    public int WindowSeconds { get; set; } = 60;
    /// <summary>Stricter per-client limit applied to credential endpoints (login, MFA, password reset, token).</summary>
    public int AuthRequestLimit { get; set; } = 10;
    public int AuthWindowSeconds { get; set; } = 60;
    public string[] WhitelistedPaths { get; set; } = Array.Empty<string>();
}
