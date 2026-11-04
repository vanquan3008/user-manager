using MediatR;
using UserService.Application.Features.Auth.Commands;
using UserService.Application.Interfaces.Services.Security;
using UserService.Application.Services;
using UserService.Domain.Entities;
using UserService.Domain.Exceptions;
using UserService.Domain.Repositories;

namespace UserService.Application.Features.Auth.Handlers;

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, Unit>
{
    private readonly IIdentityService _identityService;
    private readonly ISessionService _sessions;
    private readonly IOAuthRefreshTokenRepository _oauthTokens;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditLogger _audit;

    public ResetPasswordCommandHandler(
        IIdentityService identityService,
        ISessionService sessions,
        IOAuthRefreshTokenRepository oauthTokens,
        IUnitOfWork unitOfWork,
        IAuditLogger audit)
    {
        _identityService = identityService;
        _sessions = sessions;
        _oauthTokens = oauthTokens;
        _unitOfWork = unitOfWork;
        _audit = audit;
    }

    public async Task<Unit> Handle(ResetPasswordCommand command, CancellationToken cancellationToken)
    {
        var succeeded = await _identityService.ResetPasswordAsync(
            command.Email,
            command.Token,
            command.NewPassword,
            cancellationToken);

        if (!succeeded)
        {
            await _audit.LogAsync(AuditEventTypes.PasswordReset, false, new AuditEntry(ActorEmail: command.Email), cancellationToken);
            throw new BusinessRuleValidationException("ResetPasswordFailed", "Failed to reset password. The reset link may have expired.");
        }

        // A password reset is a credential-compromise response: end every session and OAuth grant.
        var user = await _identityService.GetUserByEmailAsync(command.Email, cancellationToken);
        if (user is not null)
        {
            var revoked = await _sessions.RevokeAllAsync(user.Id, SessionRevocationReasons.PasswordReset, cancellationToken: cancellationToken);
            await _oauthTokens.RevokeAllForUserAsync(user.Id, SessionRevocationReasons.PasswordReset, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _audit.LogAsync(AuditEventTypes.PasswordReset, true, new AuditEntry(ActorUserId: user.Id, Details: new { revokedSessions = revoked }), cancellationToken);
        }

        return Unit.Value;
    }
}
