namespace UserService.Application.Configuration;

public class AppSettings
{
    public const string SectionName = "AppSettings";
    public string ApplicationName { get; set; } = string.Empty;
    public bool IsAutoMigrate { get; set; } = false;
    public string Environment { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
    public string ApiPublicBaseUrl { get; set; } = string.Empty;
    public string FrontendBaseUrl { get; set; } = string.Empty;
    public JwtConfiguration Jwt { get; set; } = new();
    public DatabaseConfiguration Database { get; set; } = new();
    public CorsConfiguration Cors { get; set; } = new();
    public RateLimitConfiguration RateLimit { get; set; } = new();
    public OAuthConfiguration OAuth { get; set; } = new();
    public CloudinaryConfiguration Cloudinary { get; set; } = new();
    public OpenIddictConfiguration OpenIddict { get; set; } = new();
    public SwaggerConfiguration Swagger { get; set; } = new();
    
    public bool IsProduction() => Environment.Equals("Production", StringComparison.OrdinalIgnoreCase);
    public bool IsDevelopment() => Environment.Equals("Development", StringComparison.OrdinalIgnoreCase);
}