namespace UserService.Domain.Entities;

/// <summary>
/// A server-side authentication session (one per signed-in device/browser).
/// The session id is embedded in access tokens as the <c>sid</c> claim, which allows
/// immediate revocation, single logout and per-device session management.
/// Only SHA-256 hashes of refresh tokens are stored.
/// </summary>
public sealed class UserSession : Entity<Guid>
{
    private UserSession() { }

    private UserSession(
        Guid id,
        Guid userId,
        string refreshTokenHash,
        string sessionKind,
        string authenticationMethod,
        DateTime utcNow,
        DateTime idleExpiresAt,
        DateTime absoluteExpiresAt,
        string? ipAddress,
        string? userAgent,
        Guid? applicationId)
    {
        if (string.IsNullOrWhiteSpace(refreshTokenHash))
            throw new ArgumentException("Refresh token hash is required.", nameof(refreshTokenHash));

        Id = id;
        UserId = userId;
        RefreshTokenHash = refreshTokenHash;
        SessionKind = sessionKind;
        AuthenticationMethod = authenticationMethod;
        CreatedAt = utcNow;
        LastSeenAt = utcNow;
        AbsoluteExpiresAt = absoluteExpiresAt;
        ExpiresAt = idleExpiresAt < absoluteExpiresAt ? idleExpiresAt : absoluteExpiresAt;
        IpAddress = Truncate(ipAddress, 64);
        UserAgent = Truncate(userAgent, 512);
        ApplicationId = applicationId;
        Version = Guid.NewGuid();
    }

    public static UserSession Start(
        Guid userId,
        string refreshTokenHash,
        string sessionKind,
        string authenticationMethod,
        DateTime utcNow,
        TimeSpan idleLifetime,
        TimeSpan absoluteLifetime,
        string? ipAddress,
        string? userAgent,
        Guid? applicationId = null)
        => new(
            Guid.NewGuid(),
            userId,
            refreshTokenHash,
            sessionKind,
            authenticationMethod,
            utcNow,
            utcNow.Add(idleLifetime),
            utcNow.Add(absoluteLifetime),
            ipAddress,
            userAgent,
            applicationId);

    public Guid UserId { get; private set; }
    public string RefreshTokenHash { get; private set; } = string.Empty;
    public string? PreviousRefreshTokenHash { get; private set; }
    public DateTime? RotatedAt { get; private set; }
    public string SessionKind { get; private set; } = UserSessionKinds.Console;
    public string AuthenticationMethod { get; private set; } = AuthenticationMethods.Password;
    /// <summary>Application context the session was opened for (permission scoping is preserved on refresh).</summary>
    public Guid? ApplicationId { get; private set; }
    public string? IpAddress { get; private set; }
    public string? UserAgent { get; private set; }
    public DateTime LastSeenAt { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public DateTime AbsoluteExpiresAt { get; private set; }
    public DateTime? RevokedAt { get; private set; }
    public string? RevokedReason { get; private set; }

    /// <summary>Optimistic concurrency token; changes on every mutation.</summary>
    public Guid Version { get; private set; }

    public bool IsRevoked => RevokedAt.HasValue;
    public bool IsActive(DateTime utcNow) => !IsRevoked && utcNow < ExpiresAt;

    /// <summary>
    /// Rotates the refresh token. The previous hash is retained so that replay of a
    /// superseded token can be detected (refresh token reuse detection).
    /// </summary>
    public void Rotate(string newRefreshTokenHash, DateTime utcNow, TimeSpan idleLifetime, string? ipAddress, string? userAgent)
    {
        if (string.IsNullOrWhiteSpace(newRefreshTokenHash))
            throw new ArgumentException("Refresh token hash is required.", nameof(newRefreshTokenHash));
        if (!IsActive(utcNow))
            throw new InvalidOperationException("Cannot rotate an inactive session.");

        PreviousRefreshTokenHash = RefreshTokenHash;
        RefreshTokenHash = newRefreshTokenHash;
        RotatedAt = utcNow;
        LastSeenAt = utcNow;
        var idleExpiry = utcNow.Add(idleLifetime);
        ExpiresAt = idleExpiry < AbsoluteExpiresAt ? idleExpiry : AbsoluteExpiresAt;
        if (!string.IsNullOrWhiteSpace(ipAddress)) IpAddress = Truncate(ipAddress, 64);
        if (!string.IsNullOrWhiteSpace(userAgent)) UserAgent = Truncate(userAgent, 512);
        Version = Guid.NewGuid();
    }

    /// <summary>True when the superseded token was presented shortly after rotation (e.g. two browser tabs racing).</summary>
    public bool IsWithinRotationGracePeriod(DateTime utcNow, TimeSpan grace)
        => RotatedAt.HasValue && utcNow - RotatedAt.Value <= grace;

    public bool Revoke(string reason, DateTime utcNow)
    {
        if (IsRevoked)
            return false;

        RevokedAt = utcNow;
        RevokedReason = Truncate(reason, 100);
        Version = Guid.NewGuid();
        return true;
    }

    private static string? Truncate(string? value, int max)
        => string.IsNullOrEmpty(value) ? value : value.Length <= max ? value : value[..max];
}

public static class UserSessionKinds
{
    /// <summary>Admin console session (first-party SPA).</summary>
    public const string Console = "console";
    /// <summary>Hosted SSO session used to authorize third-party applications.</summary>
    public const string HostedSso = "hosted_sso";
}

public static class AuthenticationMethods
{
    public const string Password = "pwd";
    public const string MagicLink = "magic_link";
    public const string Mfa = "mfa";
    public const string Registration = "registration";
}

public static class SessionRevocationReasons
{
    public const string Logout = "logout";
    public const string UserRevoked = "user_revoked";
    public const string AdminRevoked = "admin_revoked";
    public const string RefreshTokenReuse = "refresh_token_reuse";
    public const string PasswordChanged = "password_changed";
    public const string PasswordReset = "password_reset";
    public const string AccountBanned = "account_banned";
    public const string MfaChanged = "mfa_changed";
}
