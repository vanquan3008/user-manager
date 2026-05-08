namespace UserService.Application.Configuration;

public class DatabaseConfiguration
{
    public bool EnableSensitiveDataLogging { get; set; } = false;
    public bool EnableDetailedErrors { get; set; } = false;
}
