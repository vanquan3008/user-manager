namespace UserService.Application.Interfaces.Services.Security;

public sealed record AuditEntry(
    Guid? ActorUserId = null,
    string? ActorEmail = null,
    Guid? TargetUserId = null,
    string? TargetType = null,
    string? TargetId = null,
    Guid? ApplicationId = null,
    Guid? SessionId = null,
    string? Reason = null,
    object? Details = null);

public interface IAuditLogger
{
    /// <summary>
    /// Records a security event. Request metadata (IP, user agent, correlation id, current user)
    /// is captured automatically. Never throws: audit failures are logged instead.
    /// </summary>
    Task LogAsync(string eventType, bool succeeded, AuditEntry? entry = null, CancellationToken cancellationToken = default);
}
