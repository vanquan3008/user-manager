using MediatR;
using UserService.Application.Features.Roles.Queries;
using UserService.Application.Services;

namespace UserService.Application.Features.Roles.Handlers;

public sealed class GetRoleByIdQueryHandler : IRequestHandler<GetRoleByIdQuery, RoleDto?>
{
    private readonly ISystemRoleService _roleService;

    public GetRoleByIdQueryHandler(ISystemRoleService roleService)
    {
        _roleService = roleService;
    }

    public async Task<RoleDto?> Handle(GetRoleByIdQuery query, CancellationToken cancellationToken)
    {
        return await _roleService.GetGlobalRoleByIdAsync(query.RoleId, cancellationToken);
    }
}
