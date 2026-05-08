using MediatR;
using UserService.Application.Common;
using UserService.Application.Features.Roles.Commands;
using UserService.Application.Services;

namespace UserService.Application.Features.Roles.Handlers;

public sealed class UpdateRoleCommandHandler : IRequestHandler<UpdateRoleCommand, ApplicationRoleResult>
{
    private readonly ISystemRoleService _roleService;

    public UpdateRoleCommandHandler(ISystemRoleService roleService)
    {
        _roleService = roleService;
    }

    public async Task<ApplicationRoleResult> Handle(UpdateRoleCommand command, CancellationToken cancellationToken)
    {
        return await _roleService.UpdateGlobalRoleAsync(command.RoleId, command.Description, command.Permissions, cancellationToken);
    }
}
