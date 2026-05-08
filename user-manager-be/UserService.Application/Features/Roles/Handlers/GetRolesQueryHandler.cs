using MediatR;
using UserService.Application.Features.Roles.Queries;
using UserService.Application.Interfaces.Services;
using UserService.Application.Services;

namespace UserService.Application.Features.Roles.Handlers;

public sealed class GetRolesQueryHandler : IRequestHandler<GetRolesQuery, IReadOnlyList<RoleDto>>
{
    private readonly ISystemRoleService _roleService;

    public GetRolesQueryHandler(ISystemRoleService roleService)
    {
        _roleService = roleService;
    }

    public async Task<IReadOnlyList<RoleDto>> Handle(GetRolesQuery query, CancellationToken cancellationToken)
    {
        return await _roleService.GetGlobalRolesAsync(cancellationToken);
    }
}
