using MediatR;
using UserService.Application.DTOs.Auth;
using UserService.Application.Features.Users.Commands;
using UserService.Application.Services;
using UserService.Domain.Constants;
using UserService.Domain.Exceptions;

namespace UserService.Application.Features.Users.Handlers;

public class BanUserCommandHandler : IRequestHandler<BanUserCommand, SuccessResponse>
{
    private readonly IIdentityService _identityService;

    public BanUserCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
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

        return new SuccessResponse(true, "User has been banned.");
    }
}
