using UserService.Application.Interfaces.Services.Security;
using UserService.Domain.Entities;
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
    private readonly IAuditLogger _audit;
    private readonly IIdentityService _identityService;

    public RemoveFromRoleCommandHandler(IIdentityService identityService, IAuditLogger audit)
    {
        _audit = audit;
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

        await _audit.LogAsync(AuditEventTypes.UserRoleRemoved, true, new AuditEntry(TargetUserId: command.UserId, TargetType: "user", TargetId: command.UserId.ToString(), Details: new { role = command.Role }), cancellationToken);
        return Unit.Value;
    }
}
