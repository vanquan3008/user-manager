namespace UserService.Api.Contracts.Account;

public sealed record SessionDto(
    Guid Id,
    string Kind,
    string AuthenticationMethod,
    string? IpAddress,
    string? UserAgent,
    DateTime CreatedAt,
    DateTime LastSeenAt,
    DateTime ExpiresAt,
    bool IsCurrent);

public sealed record MfaCodeApiRequest(string Code);
public sealed record MfaDisableApiRequest(string Password, string Code);
public sealed record MfaSetupDto(string SharedKey, string AuthenticatorUri);
public sealed record RecoveryCodesDto(IReadOnlyList<string> RecoveryCodes);

public sealed record AuditLogDto(
    Guid Id,
    DateTime OccurredAt,
    string EventType,
    string Category,
    bool Succeeded,
    Guid? ActorUserId,
    string? ActorEmail,
    Guid? TargetUserId,
    string? TargetType,
    string? TargetId,
    Guid? ApplicationId,
    Guid? SessionId,
    string? IpAddress,
    string? UserAgent,
    string? CorrelationId,
    string? Reason,
    string? Details);
