using MediatR;
using UserService.Application.Common;
using UserService.Application.Features.Auth.Commands;
using UserService.Application.Interfaces.Services.Security;
using UserService.Domain.Entities;
using UserService.Domain.Repositories;

namespace UserService.Application.Features.Auth.Handlers;

public class LogoutCommandHandler : IRequestHandler<LogoutCommand, Unit>
{
    private readonly ISessionService _sessions;
    private readonly IUserSessionRepository _sessionRepository;
    private readonly IAuditLogger _audit;

    public LogoutCommandHandler(ISessionService sessions, IUserSessionRepository sessionRepository, IAuditLogger audit)
    {
        _sessions = sessions;
        _sessionRepository = sessionRepository;
        _audit = audit;
    }

    public async Task<Unit> Handle(LogoutCommand command, CancellationToken cancellationToken)
    {
        var sessionId = command.SessionId;
        var userId = command.UserId;

        // Allow logout with only the refresh-token cookie (e.g. access token already expired).
        if (!sessionId.HasValue && !string.IsNullOrWhiteSpace(command.RefreshToken))
        {
            var session = await _sessionRepository.GetByRefreshTokenHashAsync(SecureTokens.Hash(command.RefreshToken.Trim()), cancellationToken);
            sessionId = session?.Id;
            userId ??= session?.UserId;
        }

        if (command.AllSessions && userId.HasValue)
        {
            var count = await _sessions.RevokeAllAsync(userId.Value, SessionRevocationReasons.Logout, cancellationToken: cancellationToken);
            await _audit.LogAsync(AuditEventTypes.Logout, true, new AuditEntry(ActorUserId: userId, Details: new { scope = "all", sessions = count }), cancellationToken);
            return Unit.Value;
        }

        if (sessionId.HasValue)
        {
            var session = await _sessions.GetAsync(sessionId.Value, cancellationToken);
            // A caller may only end its own session.
            if (session is not null && (!userId.HasValue || session.UserId == userId.Value))
            {
                await _sessions.RevokeAsync(session.Id, SessionRevocationReasons.Logout, cancellationToken);
                await _audit.LogAsync(AuditEventTypes.Logout, true, new AuditEntry(ActorUserId: session.UserId, SessionId: session.Id), cancellationToken);
            }
        }

        return Unit.Value;
    }
}
