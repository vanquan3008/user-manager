using MediatR;
using UserService.Application.Common;
using UserService.Application.Features.ApplicationRoles.Commands;
using UserService.Application.Services;

namespace UserService.Application.Features.ApplicationRoles.Handlers;

public sealed class UpdateApplicationRoleCommandHandler : IRequestHandler<UpdateApplicationRoleCommand, ApplicationRoleResult>
{
    private readonly IApplicationRoleService _roleService;

    public UpdateApplicationRoleCommandHandler(IApplicationRoleService roleService)
    {
        _roleService = roleService;
    }
    public async Task<ApplicationRoleResult> Handle(UpdateApplicationRoleCommand command, CancellationToken cancellationToken)
    {
        return await _roleService.UpdateRoleAsync(command.ApplicationId, command.RoleName, command.Description, cancellationToken);
    }
}
