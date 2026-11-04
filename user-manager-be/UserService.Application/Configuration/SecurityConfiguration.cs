namespace UserService.Application.Configuration;

public sealed class SecurityConfiguration
{
    /// <summary>Absolute session lifetime regardless of activity.</summary>
    public int SessionAbsoluteLifetimeDays { get; set; } = 30;
    /// <summary>A superseded refresh token presented within this window is rejected without revoking the session (multi-tab race).</summary>
    public int RefreshTokenReuseGraceSeconds { get; set; } = 30;
    /// <summary>How long a session's active state may be cached by the API when validating access tokens.</summary>
    public int SessionValidationCacheSeconds { get; set; } = 15;
    public int MfaChallengeMinutes { get; set; } = 5;
    public int MfaRecoveryCodeCount { get; set; } = 10;
    public int AuditLogRetentionDays { get; set; } = 365;
    /// <summary>Run the background maintenance worker (cleanup, retention, key rotation).</summary>
    public bool MaintenanceEnabled { get; set; } = true;
    /// <summary>Optional PFX used to encrypt Data Protection keys at rest (recommended in production).</summary>
    public string? DataProtectionCertificatePath { get; set; }
    public string? DataProtectionCertificatePassword { get; set; }
    public SigningKeyConfiguration SigningKeys { get; set; } = new();
    public ForwardedHeadersConfiguration ForwardedHeaders { get; set; } = new();
}

public sealed class SigningKeyConfiguration
{
    /// <summary>Automatically rotate the active signing key after this many days (0 disables automatic rotation).</summary>
    public int RotationDays { get; set; } = 90;
    /// <summary>Keep retired keys in the JWKS for this many days so already-issued tokens keep validating.</summary>
    public int RetiredKeyRetentionDays { get; set; } = 7;
    public int KeySize { get; set; } = 2048;
}

public sealed class ForwardedHeadersConfiguration
{
    /// <summary>Only honour X-Forwarded-* headers from these proxies (IP addresses).</summary>
    public List<string> KnownProxies { get; set; } = new();
    /// <summary>Only honour X-Forwarded-* headers from these networks (CIDR, e.g. 10.0.0.0/8).</summary>
    public List<string> KnownNetworks { get; set; } = new();
}
