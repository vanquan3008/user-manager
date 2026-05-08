using MediatR;
using UserService.Application.Common;
using UserService.Application.Features.Roles.Commands;
using UserService.Application.Services;

namespace UserService.Application.Features.Roles.Handlers;

public sealed class DeleteRoleCommandHandler : IRequestHandler<DeleteRoleCommand, ApplicationRoleResult>
{
    private readonly ISystemRoleService _roleService;

    public DeleteRoleCommandHandler(ISystemRoleService roleService)
    {
        _roleService = roleService;
    }

    public async Task<ApplicationRoleResult> Handle(DeleteRoleCommand command, CancellationToken cancellationToken)
    {
        return await _roleService.DeleteGlobalRoleAsync(command.RoleId, cancellationToken);
    }
}
