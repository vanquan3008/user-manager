using UserService.Application.Interfaces.Services.Security;
using UserService.Domain.Entities;
using MediatR;
using UserService.Application.Features.Users.Commands;
using UserService.Application.Services;
using UserService.Domain.Exceptions;

namespace UserService.Application.Features.Users.Handlers;

/// <summary>
/// Handler for AddToRoleCommand - assigns a global role to a user.
/// Role must exist in AspNetRoles (ApplicationId=null).
/// </summary>
public class AddToRoleCommandHandler : IRequestHandler<AddToRoleCommand, Unit>
{
    private readonly IAuditLogger _audit;
    private readonly IIdentityService _identityService;
    private readonly ISystemRoleService _roleService;

    public AddToRoleCommandHandler(IIdentityService identityService, ISystemRoleService roleService, IAuditLogger audit)
    {
        _audit = audit;
        _identityService = identityService;
        _roleService = roleService;
    }

    public async Task<Unit> Handle(
        AddToRoleCommand command,
        CancellationToken cancellationToken)
    {
        var roleDto = await _roleService.GetGlobalRoleByNameAsync(command.Role, cancellationToken);
        if (roleDto == null)
        {
            throw new InvalidRoleException(command.Role);
        }

        var succeeded = await _identityService.AddToRoleAsync(
            command.UserId,
            roleDto.Name,
            cancellationToken);

        if (!succeeded)
        {
            throw new UserRoleOperationException(
                command.UserId,
                roleDto.Name,
                "assign",
                "User may not exist or role assignment failed");
        }

        await _audit.LogAsync(AuditEventTypes.UserRoleAdded, true, new AuditEntry(TargetUserId: command.UserId, TargetType: "user", TargetId: command.UserId.ToString(), Details: new { role = roleDto.Name }), cancellationToken);
        return Unit.Value;
    }
}
