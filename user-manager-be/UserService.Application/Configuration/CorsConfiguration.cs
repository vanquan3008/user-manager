namespace UserService.Application.Configuration;

public class CorsConfiguration
{
    public string[] AllowedOrigins { get; set; } = Array.Empty<string>();
    public bool AllowCredentials { get; set; } = false;
}
