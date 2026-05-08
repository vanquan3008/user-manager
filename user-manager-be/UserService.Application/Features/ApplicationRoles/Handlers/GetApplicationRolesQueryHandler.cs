using MediatR;
using UserService.Application.Features.ApplicationRoles.Queries;
using UserService.Application.Interfaces.Services;

namespace UserService.Application.Features.ApplicationRoles.Handlers;
public sealed class GetApplicationRolesQueryHandler : IRequestHandler<GetApplicationRolesQuery, IReadOnlyList<ApplicationRoleDto>>
{
    private readonly IApplicationRoleService _roleService;

    public GetApplicationRolesQueryHandler(IApplicationRoleService roleService)
    {
        _roleService = roleService;
    }
    public async Task<IReadOnlyList<ApplicationRoleDto>> Handle(GetApplicationRolesQuery query, CancellationToken cancellationToken)
    {
        return await _roleService.GetRolesForApplicationAsync(query.ApplicationId, cancellationToken);
    }
}
