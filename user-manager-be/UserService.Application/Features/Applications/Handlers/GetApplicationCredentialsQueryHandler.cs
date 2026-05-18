using MediatR;
using UserService.Application.DTOs.Applications;
using UserService.Application.Features.Applications.Queries;
using UserService.Application.Interfaces.Services;

namespace UserService.Application.Features.Applications.Handlers;

public sealed class GetApplicationCredentialsQueryHandler
    : IRequestHandler<GetApplicationCredentialsQuery, ApplicationCredentialDto>
{
    private readonly IApplicationAppService _applicationService;

    public GetApplicationCredentialsQueryHandler(IApplicationAppService applicationService)
    {
        _applicationService = applicationService;
    }

    public Task<ApplicationCredentialDto> Handle(GetApplicationCredentialsQuery request, CancellationToken cancellationToken)
        => _applicationService.GetCredentialsAsync(request.ApplicationId, cancellationToken);
}
