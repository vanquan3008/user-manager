using MediatR;
using UserService.Application.Features.Users.Commands;
using UserService.Application.Services;
using UserService.Domain.Exceptions;

namespace UserService.Application.Features.Users.Handlers;

/// <summary>
/// Handler for RemoveFromRoleCommand - removes a role from a user.
/// Throws domain exceptions on business rule violations.
/// </summary>
public class RemoveFromRoleCommandHandler : IRequestHandler<RemoveFromRoleCommand, Unit>
{
    private readonly IIdentityService _identityService;

    public RemoveFromRoleCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<Unit> Handle(
        RemoveFromRoleCommand command,
        CancellationToken cancellationToken)
    {
        var succeeded = await _identityService.RemoveFromRoleAsync(
            command.UserId,
            command.Role,
            cancellationToken);

        if (!succeeded)
        {
            throw new UserRoleOperationException(
                command.UserId,
                command.Role,
                "remove",
                "User may not exist or role removal failed");
        }

        return Unit.Value;
    }
}
