namespace UserService.Application.Configuration;

public class RateLimitConfiguration
{
    public int RequestLimit { get; set; } = 100;
    public int WindowSeconds { get; set; } = 60;
    public string[] WhitelistedPaths { get; set; } = Array.Empty<string>();
}
