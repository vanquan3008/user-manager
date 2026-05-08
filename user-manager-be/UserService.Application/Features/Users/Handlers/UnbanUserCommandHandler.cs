using MediatR;
using UserService.Application.DTOs.Auth;
using UserService.Application.Features.Users.Commands;
using UserService.Application.Services;
using UserService.Domain.Constants;
using UserService.Domain.Exceptions;

namespace UserService.Application.Features.Users.Handlers;

public class UnbanUserCommandHandler : IRequestHandler<UnbanUserCommand, SuccessResponse>
{
    private readonly IIdentityService _identityService;

    public UnbanUserCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<SuccessResponse> Handle(UnbanUserCommand command, CancellationToken cancellationToken)
    {
        var targetRoles = await _identityService.GetUserRolesAsync(command.UserId, cancellationToken);
        var actorRoles = await _identityService.GetUserRolesAsync(command.ActorUserId, cancellationToken);

        if (targetRoles.Contains(RoleNames.SuperAdmin) && !actorRoles.Contains(RoleNames.SuperAdmin))
            throw new BusinessRuleValidationException("SuperAdminProtection", "Only SuperAdmin can unban another SuperAdmin.");

        var (ok, errors) = await _identityService.UnbanUserAsync(command.UserId, null, cancellationToken);
        if (!ok)
            throw new BusinessRuleValidationException("UnbanUser", string.Join("; ", errors));

        return new SuccessResponse(true, "User has been unbanned.");
    }
}
