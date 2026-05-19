using MediatR;
using UserService.Application.DTOs.Users;
using UserService.Application.Features.Users.Queries;
using UserService.Application.Interfaces.Services;

namespace UserService.Application.Features.Users.Handlers;

public sealed class GetSSOUserApplicationsQueryHandler(IApplicationMembershipService membershipService)
    : IRequestHandler<GetSSOUserApplicationsQuery, IReadOnlyList<SSOUserApplicationDto>>
{
    public async Task<IReadOnlyList<SSOUserApplicationDto>> Handle(
        GetSSOUserApplicationsQuery request,
        CancellationToken cancellationToken)
    {
        var memberships = await membershipService.GetByUserAsync(request.UserId, cancellationToken);

        return memberships
            .Where(membership => membership.IsActive)
            .OrderBy(membership => membership.Application.Name)
            .Select(membership => new SSOUserApplicationDto(
                membership.ApplicationId,
                membership.Application.Name,
                membership.RoleId.ToString()))
            .ToList();
    }
}
