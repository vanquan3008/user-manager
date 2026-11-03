using MediatR;
using UserService.Application.Features.Applications.Queries;
using UserService.Application.Mapping;
using UserService.Domain.Repositories;

namespace UserService.Application.Features.Applications.Handlers;

public sealed class GetApplicationUsersQueryHandler
    : IRequestHandler<GetApplicationUsersQuery, IReadOnlyList<ApplicationUserDto>>
{
    private readonly IApplicationMembershipRepository _memberships;
    private readonly UserService.Application.Interfaces.Services.ISystemRoleService _roles;

    public GetApplicationUsersQueryHandler(
        IApplicationMembershipRepository memberships,
        UserService.Application.Interfaces.Services.ISystemRoleService roles)
    {
        _memberships = memberships;
        _roles = roles;
    }

    public async Task<IReadOnlyList<ApplicationUserDto>> Handle(GetApplicationUsersQuery request, CancellationToken cancellationToken)
    {
        var memberships = (await _memberships.GetByApplicationAsync(request.ApplicationId, cancellationToken))
            .ToList();

        if (memberships.Count == 0)
            return Array.Empty<ApplicationUserDto>();

        // Memberships reference roles by id; return the role name the UI and API consumers expect.
        var roleNames = (await _roles.GetGlobalRolesAsync(cancellationToken)).ToDictionary(r => r.Id, r => r.Name);
        return memberships
            .Select(m => ApplicationEntityMapper.ToApplicationUserDto(m) with
            {
                Role = roleNames.TryGetValue(m.RoleId, out var name) ? name : m.RoleId.ToString()
            })
            .ToList();
    }
}
