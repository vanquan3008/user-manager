using MediatR;
using UserService.Application.DTOs.Applications;
using UserService.Application.Features.Applications.Queries;
using UserService.Application.Mapping;
using UserService.Domain.Exceptions;
using UserService.Domain.Repositories;

namespace UserService.Application.Features.Applications.Handlers;

public sealed class GetApplicationByIdQueryHandler
    : IRequestHandler<GetApplicationByIdQuery, CreateApplicationResponse>
{
    private readonly IApplicationRepository _applications;

    public GetApplicationByIdQueryHandler(IApplicationRepository applications)
    {
        _applications = applications;
    }

    public async Task<CreateApplicationResponse> Handle(GetApplicationByIdQuery request, CancellationToken cancellationToken)
    {
        var application = await _applications.GetByIdWithOwnerAsync(request.ApplicationId, cancellationToken);
        if (application is null || application.IsDeleted)
        {
            throw new ApplicationNotFoundException(request.ApplicationId);
        }

        return ApplicationEntityMapper.ToCreateApplicationResponse(application);
    }
}
