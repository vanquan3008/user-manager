namespace UserService.Application.Configuration;

public class SwaggerConfiguration
{
    /// <summary>
    /// Whether Swagger UI is enabled
    /// Should be false in production or protected by authentication
    /// </summary>
    public bool Enabled { get; set; } = true;
    
    /// <summary>
    /// Whether to require authentication to access Swagger UI
    /// </summary>
    public bool RequireAuthentication { get; set; } = false;
    
    /// <summary>
    /// List of IP addresses allowed to access Swagger UI
    /// Empty list means all IPs are allowed
    /// </summary>
    public List<string> AllowedIpAddresses { get; set; } = new();
}
