using MediatR;
using UserService.Application.DTOs.Applications;
using UserService.Application.Features.Applications.Commands;
using UserService.Application.Interfaces.Services;

namespace UserService.Application.Features.Applications.Handlers;

public sealed class RotateApplicationClientSecretCommandHandler
    : IRequestHandler<RotateApplicationClientSecretCommand, ApplicationCredentialDto>
{
    private readonly IApplicationAppService _applicationService;

    public RotateApplicationClientSecretCommandHandler(IApplicationAppService applicationService)
    {
        _applicationService = applicationService;
    }

    public Task<ApplicationCredentialDto> Handle(
        RotateApplicationClientSecretCommand request,
        CancellationToken cancellationToken)
        => _applicationService.RotateClientSecretAsync(request.ApplicationId, cancellationToken);
}
