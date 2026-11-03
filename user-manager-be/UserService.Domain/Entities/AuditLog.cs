namespace UserService.Domain.Entities;

/// <summary>
/// Immutable security audit record (authentication history, administrative changes).
/// </summary>
public sealed class AuditLog
{
    private AuditLog() { }

    public Guid Id { get; private set; }
    public DateTime OccurredAt { get; private set; }
    public string EventType { get; private set; } = string.Empty;
    public string Category { get; private set; } = string.Empty;
    public bool Succeeded { get; private set; }
    public Guid? ActorUserId { get; private set; }
    public string? ActorEmail { get; private set; }
    public Guid? TargetUserId { get; private set; }
    public string? TargetType { get; private set; }
    public string? TargetId { get; private set; }
    public Guid? ApplicationId { get; private set; }
    public Guid? SessionId { get; private set; }
    public string? IpAddress { get; private set; }
    public string? UserAgent { get; private set; }
    public string? CorrelationId { get; private set; }
    public string? Reason { get; private set; }
    public string? Details { get; private set; }

    public static AuditLog Create(
        string eventType,
        bool succeeded,
        DateTime occurredAt,
        Guid? actorUserId = null,
        string? actorEmail = null,
        Guid? targetUserId = null,
        string? targetType = null,
        string? targetId = null,
        Guid? applicationId = null,
        Guid? sessionId = null,
        string? ipAddress = null,
        string? userAgent = null,
        string? correlationId = null,
        string? reason = null,
        string? details = null)
    {
        if (string.IsNullOrWhiteSpace(eventType))
            throw new ArgumentException("Event type is required.", nameof(eventType));

        return new AuditLog
        {
            Id = Guid.NewGuid(),
            OccurredAt = occurredAt,
            EventType = Truncate(eventType, 100)!,
            Category = Truncate(AuditEventTypes.CategoryOf(eventType), 50)!,
            Succeeded = succeeded,
            ActorUserId = actorUserId,
            ActorEmail = Truncate(actorEmail?.Trim().ToLowerInvariant(), 256),
            TargetUserId = targetUserId,
            TargetType = Truncate(targetType, 50),
            TargetId = Truncate(targetId, 100),
            ApplicationId = applicationId,
            SessionId = sessionId,
            IpAddress = Truncate(ipAddress, 64),
            UserAgent = Truncate(userAgent, 512),
            CorrelationId = Truncate(correlationId, 100),
            Reason = Truncate(reason, 500),
            Details = Truncate(details, 4000)
        };
    }

    private static string? Truncate(string? value, int max)
        => string.IsNullOrEmpty(value) ? value : value.Length <= max ? value : value[..max];
}

/// <summary>Stable audit event identifiers. Format: <c>category.action[.qualifier]</c>.</summary>
public static class AuditEventTypes
{
    public const string LoginSucceeded = "auth.login.succeeded";
    public const string LoginFailed = "auth.login.failed";
    public const string LoginMfaChallenged = "auth.login.mfa_challenged";
    public const string MfaVerified = "auth.mfa.verified";
    public const string MfaFailed = "auth.mfa.failed";
    public const string MagicLinkLogin = "auth.magic_link.login";
    public const string Registered = "auth.registered";
    public const string Logout = "auth.logout";
    public const string TokenRefreshed = "auth.token.refreshed";
    public const string RefreshTokenReuseDetected = "auth.token.reuse_detected";
    public const string PasswordChanged = "auth.password.changed";
    public const string PasswordResetRequested = "auth.password.reset_requested";
    public const string PasswordReset = "auth.password.reset";

    public const string SessionRevoked = "session.revoked";
    public const string SessionsRevokedAll = "session.revoked_all";

    public const string MfaEnabled = "mfa.enabled";
    public const string MfaDisabled = "mfa.disabled";
    public const string MfaRecoveryCodesRegenerated = "mfa.recovery_codes.regenerated";

    public const string OAuthCodeIssued = "oauth.code.issued";
    public const string OAuthTokenIssued = "oauth.token.issued";
    public const string OAuthTokenFailed = "oauth.token.failed";
    public const string OAuthCodeReplay = "oauth.code.replay_detected";
    public const string OAuthRefreshReuse = "oauth.refresh.reuse_detected";
    public const string OAuthConsentGranted = "oauth.consent.granted";
    public const string OAuthTokensRevoked = "oauth.tokens.revoked";

    public const string UserBanned = "user.banned";
    public const string UserUnbanned = "user.unbanned";
    public const string UserRoleAdded = "user.role.added";
    public const string UserRoleRemoved = "user.role.removed";
    public const string SsoUserCreated = "user.sso.created";

    public const string ApplicationCreated = "application.created";
    public const string ApplicationDeleted = "application.deleted";
    public const string ApplicationSecretRotated = "application.secret.rotated";

    public const string SigningKeyRotated = "security.signing_key.rotated";
    public const string SigningKeyRevoked = "security.signing_key.revoked";

    public static string CategoryOf(string eventType)
    {
        var dot = eventType.IndexOf('.');
        return dot > 0 ? eventType[..dot] : eventType;
    }
}
