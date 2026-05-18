using MediatR;
using UserService.Application.Features.Applications.Queries;
using UserService.Application.Mapping;
using UserService.Domain.Repositories;

namespace UserService.Application.Features.Applications.Handlers;

public sealed class GetApplicationUsersQueryHandler
    : IRequestHandler<GetApplicationUsersQuery, IReadOnlyList<ApplicationUserDto>>
{
    private readonly IApplicationMembershipRepository _memberships;

    public GetApplicationUsersQueryHandler(IApplicationMembershipRepository memberships)
    {
        _memberships = memberships;
    }

    public async Task<IReadOnlyList<ApplicationUserDto>> Handle(GetApplicationUsersQuery request, CancellationToken cancellationToken)
    {
        var memberships = (await _memberships.GetByApplicationAsync(request.ApplicationId, cancellationToken))
            .ToList();

        if (memberships.Count == 0)
            return Array.Empty<ApplicationUserDto>();

        return memberships.Select(ApplicationEntityMapper.ToApplicationUserDto).ToList();
    }
}
