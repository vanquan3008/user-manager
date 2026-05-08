using MediatR;
using UserService.Application.Common;
using UserService.Application.Features.Roles.Commands;
using UserService.Application.Services;

namespace UserService.Application.Features.Roles.Handlers;

public sealed class CreateRoleCommandHandler : IRequestHandler<CreateRoleCommand, ApplicationRoleResult>
{
    private readonly ISystemRoleService _roleService;

    public CreateRoleCommandHandler(ISystemRoleService roleService)
    {
        _roleService = roleService;
    }

    public async Task<ApplicationRoleResult> Handle(CreateRoleCommand command, CancellationToken cancellationToken)
    {
        return await _roleService.CreateGlobalRoleAsync(command.Name, command.Description, command.Permissions, cancellationToken);
    }
}
