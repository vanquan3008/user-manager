using MediatR;
using UserService.Application.DTOs.Applications;
using UserService.Application.Features.Applications.Queries;
using UserService.Application.Mapping;
using UserService.Domain.Entities;
using UserService.Domain.Repositories;

namespace UserService.Application.Features.Applications.Handlers;

public sealed class GetApplicationsQueryHandler
    : IRequestHandler<GetApplicationsQuery, IReadOnlyList<CreateApplicationResponse>>
{
    private readonly IApplicationRepository _applications;

    public GetApplicationsQueryHandler(IApplicationRepository applications)
    {
        _applications = applications;
    }

    public async Task<IReadOnlyList<CreateApplicationResponse>> Handle(GetApplicationsQuery request, CancellationToken cancellationToken)
    {
        IReadOnlyList<AppApplication> apps;

        if (request.IsAdmin)
        {
            apps = await _applications.GetAllActiveWithOwnerAsync(cancellationToken);
        }
        else if (request.UserId.HasValue)
        {
            var ownedApps = await _applications.GetByOwnerAsync(request.UserId.Value, cancellationToken);
            var memberApps = await _applications.GetByUserAsync(request.UserId.Value, cancellationToken);

            apps = ownedApps
                .Concat(memberApps)
                .Where(app => !app.IsDeleted)
                .GroupBy(app => app.Id)
                .Select(group => group.First())
                .ToList();
        }
        else
        {
            apps = Array.Empty<AppApplication>();
        }

        return apps.Select(app => ApplicationEntityMapper.ToCreateApplicationResponse(app)).ToList();
    }
}
