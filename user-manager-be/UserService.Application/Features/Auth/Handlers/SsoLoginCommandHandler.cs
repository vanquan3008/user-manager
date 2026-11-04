using MediatR;
using UserService.Application.Features.Auth.Commands;
using UserService.Application.Interfaces.Services;
using UserService.Application.Interfaces.Services.Security;
using UserService.Application.Services.Auth;
using UserService.Domain.Constants;
using UserService.Domain.Entities;
using UserService.Domain.Exceptions;
using UserService.Domain.Repositories;

namespace UserService.Application.Features.Auth.Handlers;

/// <summary>Signs a user into the hosted SSO portal (used to authorize third-party applications).</summary>
public sealed class SsoLoginCommandHandler : IRequestHandler<SsoLoginCommand, SsoLoginResponse>
{
    private readonly IIdentityService _identityService;
    private readonly IApplicationMembershipRepository _membershipRepository;
    private readonly AuthResponseFactory _responses;
    private readonly IAuditLogger _audit;

    public SsoLoginCommandHandler(
        IIdentityService identityService,
        IApplicationMembershipRepository membershipRepository,
        AuthResponseFactory responses,
        IAuditLogger audit)
    {
        _identityService = identityService;
        _membershipRepository = membershipRepository;
        _responses = responses;
        _audit = audit;
    }

    public async Task<SsoLoginResponse> Handle(SsoLoginCommand request, CancellationToken cancellationToken)
    {
        var result = await _identityService.LoginAsync(request.Email, request.Password, cancellationToken);
        if (!result.Succeeded || result.User == null)
        {
            await _audit.LogAsync(AuditEventTypes.LoginFailed, false,
                new AuditEntry(ActorEmail: request.Email, Reason: result.ErrorCode, Details: new { kind = UserSessionKinds.HostedSso }), cancellationToken);
            throw new AuthenticationFlowException(result.ErrorCode ?? AuthErrorCodes.InvalidCredentials, result.Message ?? "Invalid email or password.");
        }

        var user = result.User;
        if (!user.IsSSOAccount)
        {
            await _audit.LogAsync(AuditEventTypes.LoginFailed, false,
                new AuditEntry(ActorUserId: user.Id, ActorEmail: request.Email, Reason: AuthErrorCodes.WrongLoginEndpoint), cancellationToken);
            throw new AuthenticationFlowException(AuthErrorCodes.WrongLoginEndpoint,
                "This account uses the administrator console. Please sign in from /login.");
        }

        var roles = await _identityService.GetUserRolesAsync(user.Id, cancellationToken);
        if (!RoleNames.ContainsSystemAdministrator(roles))
        {
            var memberships = await _membershipRepository.GetByUserAsync(user.Id, cancellationToken);
            if (!memberships.Any(m => m.IsActive))
            {
                await _audit.LogAsync(AuditEventTypes.LoginFailed, false,
                    new AuditEntry(ActorUserId: user.Id, ActorEmail: request.Email, Reason: "no_application_access"), cancellationToken);
                throw new UnauthorizedException("SSO user has no active application access.");
            }
        }

        return await _responses.CompleteHostedSsoSignInAsync(user, AuthenticationMethods.Password, cancellationToken);
    }
}
