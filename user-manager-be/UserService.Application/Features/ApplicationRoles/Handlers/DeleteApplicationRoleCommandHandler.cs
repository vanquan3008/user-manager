using MediatR;
using UserService.Application.Common;
using UserService.Application.Features.ApplicationRoles.Commands;
using UserService.Application.Interfaces.Services;

namespace UserService.Application.Features.ApplicationRoles.Handlers;

public sealed class DeleteApplicationRoleCommandHandler : IRequestHandler<DeleteApplicationRoleCommand, ApplicationRoleResult>
{
    private readonly IApplicationRoleService _roleService;
    public DeleteApplicationRoleCommandHandler(IApplicationRoleService roleService)
    {
        _roleService = roleService;
    }
    public async Task<ApplicationRoleResult> Handle(DeleteApplicationRoleCommand command, CancellationToken cancellationToken)
    {
        return await _roleService.DeleteRoleAsync(command.ApplicationId, command.RoleName, cancellationToken);
    }
}
