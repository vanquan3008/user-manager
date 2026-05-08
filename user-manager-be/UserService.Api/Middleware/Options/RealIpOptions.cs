namespace UserService.Api.Middleware.Options;

/// <summary>
/// Configuration options for RealIpMiddleware
/// </summary>
public class RealIpOptions
{
    /// <summary>
    /// Enable debug logging of IP extraction
    /// </summary>
    public bool LogIpExtraction { get; set; } = false;

    /// <summary>
    /// Trusted proxy IP addresses (for validation)
    /// </summary>
    public HashSet<string> TrustedProxies { get; set; } = new();
}
