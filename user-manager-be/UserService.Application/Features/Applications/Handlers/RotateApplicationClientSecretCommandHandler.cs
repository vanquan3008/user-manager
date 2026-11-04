using MediatR;
using UserService.Application.DTOs.Applications;
using UserService.Application.Features.Applications.Commands;
using UserService.Application.Interfaces.Services;
using UserService.Application.Interfaces.Services.Security;
using UserService.Domain.Entities;

namespace UserService.Application.Features.Applications.Handlers;

public sealed class RotateApplicationClientSecretCommandHandler
    : IRequestHandler<RotateApplicationClientSecretCommand, ApplicationCredentialDto>
{
    private readonly IApplicationAppService _applicationService;
    private readonly IAuditLogger _audit;

    public RotateApplicationClientSecretCommandHandler(IApplicationAppService applicationService, IAuditLogger audit)
    {
        _applicationService = applicationService;
        _audit = audit;
    }

    public async Task<ApplicationCredentialDto> Handle(
        RotateApplicationClientSecretCommand request,
        CancellationToken cancellationToken)
    {
        var result = await _applicationService.RotateClientSecretAsync(request.ApplicationId, cancellationToken);
        await _audit.LogAsync(AuditEventTypes.ApplicationSecretRotated, true, new AuditEntry(
            TargetType: "application", TargetId: request.ApplicationId.ToString(), ApplicationId: request.ApplicationId), cancellationToken);
        return result;
    }
}
