using MediatR;
using UserService.Application.DTOs.Users;
using UserService.Application.Features.Users.Queries;
using UserService.Application.Services;

namespace UserService.Application.Features.Users.Handlers;

public sealed class GetSsoUserApplicationsQueryHandler(IUserApplicationAppService userApplicationService)
    : IRequestHandler<GetSsoUserApplicationsQuery, IReadOnlyList<SsoUserApplicationDto>>
{
    public async Task<IReadOnlyList<SsoUserApplicationDto>> Handle(
        GetSsoUserApplicationsQuery request,
        CancellationToken cancellationToken)
    {
        var memberships = await userApplicationService.GetByUserAsync(request.UserId, cancellationToken);

        return memberships
            .Where(membership => membership.IsActive)
            .OrderBy(membership => membership.Application.Name)
            .Select(membership => new SsoUserApplicationDto(
                membership.ApplicationId,
                membership.Application.Name,
                membership.Role))
            .ToList();
    }
}
