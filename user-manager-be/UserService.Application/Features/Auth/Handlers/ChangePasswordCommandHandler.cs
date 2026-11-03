using MediatR;
using Microsoft.Extensions.Logging;
using UserService.Application.Features.Auth.Commands;
using UserService.Application.Interfaces.Services.Security;
using UserService.Application.Services;
using UserService.Domain.Entities;
using UserService.Domain.Exceptions;

namespace UserService.Application.Features.Auth.Handlers;

public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, Unit>
{
    private readonly IIdentityService _identityService;
    private readonly ISessionService _sessions;
    private readonly IRequestContext _requestContext;
    private readonly IAuditLogger _audit;
    private readonly ILogger<ChangePasswordCommandHandler> _logger;

    public ChangePasswordCommandHandler(
        IIdentityService identityService,
        ISessionService sessions,
        IRequestContext requestContext,
        IAuditLogger audit,
        ILogger<ChangePasswordCommandHandler> logger)
    {
        _identityService = identityService;
        _sessions = sessions;
        _requestContext = requestContext;
        _audit = audit;
        _logger = logger;
    }

    public async Task<Unit> Handle(ChangePasswordCommand command, CancellationToken cancellationToken)
    {
        var succeeded = await _identityService.ChangePasswordAsync(
            command.UserId,
            command.CurrentPassword,
            command.NewPassword,
            cancellationToken);

        if (!succeeded)
        {
            await _audit.LogAsync(AuditEventTypes.PasswordChanged, false, new AuditEntry(ActorUserId: command.UserId), cancellationToken);
            throw new BusinessRuleValidationException("ChangePasswordFailed", "Failed to change password. Verify your current password is correct.");
        }

        // Sign out every other device; the current session stays signed in.
        var revoked = await _sessions.RevokeAllAsync(command.UserId, SessionRevocationReasons.PasswordChanged, _requestContext.SessionId, cancellationToken);
        await _audit.LogAsync(AuditEventTypes.PasswordChanged, true, new AuditEntry(ActorUserId: command.UserId, Details: new { revokedSessions = revoked }), cancellationToken);
        _logger.LogInformation("Password changed for user {UserId}; {Count} other sessions revoked", command.UserId, revoked);
        return Unit.Value;
    }
}
