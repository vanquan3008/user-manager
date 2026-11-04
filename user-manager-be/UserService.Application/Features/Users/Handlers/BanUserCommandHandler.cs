using MediatR;
using UserService.Application.DTOs.Auth;
using UserService.Application.Features.Users.Commands;
using UserService.Application.Interfaces.Services.Security;
using UserService.Application.Services;
using UserService.Domain.Constants;
using UserService.Domain.Entities;
using UserService.Domain.Exceptions;
using UserService.Domain.Repositories;

namespace UserService.Application.Features.Users.Handlers;

public class BanUserCommandHandler : IRequestHandler<BanUserCommand, SuccessResponse>
{
    private readonly IIdentityService _identityService;
    private readonly ISessionService _sessions;
    private readonly IOAuthRefreshTokenRepository _oauthTokens;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditLogger _audit;

    public BanUserCommandHandler(
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

    public async Task<SuccessResponse> Handle(BanUserCommand command, CancellationToken cancellationToken)
    {
        if (command.UserId == command.ActorUserId)
            throw new BusinessRuleValidationException("SelfBan", "Cannot ban yourself.");

        var targetRoles = await _identityService.GetUserRolesAsync(command.UserId, cancellationToken);
        var actorRoles = await _identityService.GetUserRolesAsync(command.ActorUserId, cancellationToken);

        if (targetRoles.Contains(RoleNames.SuperAdmin) && !actorRoles.Contains(RoleNames.SuperAdmin))
            throw new BusinessRuleValidationException("SuperAdminProtection", "Only SuperAdmin can ban another SuperAdmin.");

        var (ok, errors) = await _identityService.BanUserAsync(command.UserId, command.Reason, cancellationToken);
        if (!ok)
            throw new BusinessRuleValidationException("BanUser", string.Join("; ", errors));

        // Banning takes effect immediately: revoke sessions (access tokens fail validation) and OAuth grants.
        var revoked = await _sessions.RevokeAllAsync(command.UserId, SessionRevocationReasons.AccountBanned, cancellationToken: cancellationToken);
        await _oauthTokens.RevokeAllForUserAsync(command.UserId, SessionRevocationReasons.AccountBanned, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _audit.LogAsync(AuditEventTypes.UserBanned, true, new AuditEntry(
            ActorUserId: command.ActorUserId, TargetUserId: command.UserId, TargetType: "user", TargetId: command.UserId.ToString(),
            Reason: command.Reason, Details: new { revokedSessions = revoked }), cancellationToken);

        return new SuccessResponse(true, "User has been banned.");
    }
}
