using System.Text.Json;
using Microsoft.Extensions.Logging;
using UserService.Application.Interfaces.Services.Security;
using UserService.Domain.Entities;
using UserService.Domain.Interfaces;
using UserService.Domain.Repositories;
using UserService.Application.Interfaces.Services;

namespace UserService.Application.Services.Security;

public sealed class AuditLogger : IAuditLogger
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly IAuditLogRepository _repository;
    private readonly IRequestContext _requestContext;
    private readonly IClock _clock;
    private readonly ILogger<AuditLogger> _logger;
    private readonly IUserRepository _users;

    public AuditLogger(IAuditLogRepository repository, IRequestContext requestContext, IClock clock, ILogger<AuditLogger> logger, IUserRepository users)
    {
        _users = users;
        _repository = repository;
        _requestContext = requestContext;
        _clock = clock;
        _logger = logger;
    }

    public async Task LogAsync(string eventType, bool succeeded, AuditEntry? entry = null, CancellationToken cancellationToken = default)
    {
        entry ??= new AuditEntry();
        try
        {
            var actorId = entry.ActorUserId ?? _requestContext.UserId;
            var actorEmail = entry.ActorEmail ?? (entry.ActorUserId is null ? _requestContext.UserEmail : null);
            if (actorEmail is null && actorId.HasValue)
            {
                // Denormalise the email so the audit trail stays readable (and survives user deletion).
                var user = await _users.GetByIdAsync(actorId.Value, CancellationToken.None);
                actorEmail = user?.Email.Value;
            }
            var record = AuditLog.Create(
                eventType,
                succeeded,
                _clock.UtcNow,
                actorUserId: actorId,
                actorEmail: actorEmail,
                targetUserId: entry.TargetUserId ?? (entry.TargetType is null ? actorId : null),
                targetType: entry.TargetType,
                targetId: entry.TargetId,
                applicationId: entry.ApplicationId,
                sessionId: entry.SessionId ?? _requestContext.SessionId,
                ipAddress: _requestContext.IpAddress,
                userAgent: _requestContext.UserAgent,
                correlationId: _requestContext.CorrelationId,
                reason: entry.Reason,
                details: entry.Details is null ? null : JsonSerializer.Serialize(entry.Details, JsonOptions));

            // CancellationToken.None: an aborted request must not prevent the security record from being written.
            await _repository.WriteAsync(record, CancellationToken.None);

            _logger.Log(
                succeeded ? LogLevel.Information : LogLevel.Warning,
                "Audit {EventType} succeeded={Succeeded} actor={ActorUserId} target={TargetType}:{TargetId} ip={IpAddress}",
                eventType, succeeded, actorId, entry.TargetType, entry.TargetId, _requestContext.IpAddress);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to persist audit event {EventType}", eventType);
        }
    }
}
