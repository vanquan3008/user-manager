namespace UserService.Application.Configuration;

public class OpenIddictConfiguration
{
    /// <summary>
    /// Path to the encryption certificate file (.pfx)
    /// Required for production environments
    /// </summary>
    public string? EncryptionCertificatePath { get; set; }
    
    /// <summary>
    /// Password for the encryption certificate
    /// </summary>
    public string? EncryptionCertificatePassword { get; set; }
    
    /// <summary>
    /// Path to the signing certificate file (.pfx)
    /// Required for production environments
    /// </summary>
    public string? SigningCertificatePath { get; set; }
    
    /// <summary>
    /// Password for the signing certificate
    /// </summary>
    public string? SigningCertificatePassword { get; set; }
    
    /// <summary>
    /// Whether to accept anonymous clients (public clients without client_secret)
    /// Should be false in production unless you have specific requirements for SPAs
    /// </summary>
    public bool AcceptAnonymousClients { get; set; } = false;
    
    /// <summary>
    /// Whether to allow password flow
    /// Consider disabling in production and using authorization code flow with PKCE instead
    /// </summary>
    public bool AllowPasswordFlow { get; set; } = false;
    
    /// <summary>
    /// Whether to allow refresh token flow
    /// </summary>
    public bool AllowRefreshTokenFlow { get; set; } = true;
    
    /// <summary>
    /// Whether to allow authorization code flow
    /// </summary>
    public bool AllowAuthorizationCodeFlow { get; set; } = true;
    
    /// <summary>
    /// Whether to allow custom magic_link flow
    /// </summary>
    public bool AllowMagicLinkFlow { get; set; } = true;
    
    /// <summary>
    /// Whether to require PKCE for authorization code flow
    /// Should always be true for security
    /// </summary>
    public bool RequireProofKeyForCodeExchange { get; set; } = true;
}
