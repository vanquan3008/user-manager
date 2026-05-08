namespace UserService.Application.Configuration;

public class OAuthConfiguration
{
    public string[] AllowedRedirectUriHosts { get; set; } = Array.Empty<string>();
    public string[] DefaultApplicationAllowedRedirectUris { get; set; } = Array.Empty<string>();
    public bool AllowHttpForLocalhost { get; set; } = true;
}
