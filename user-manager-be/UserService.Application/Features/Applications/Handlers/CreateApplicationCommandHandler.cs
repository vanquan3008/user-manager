using MediatR;
using UserService.Application.DTOs.Applications;
using UserService.Application.Features.Applications.Commands;
using UserService.Application.Interfaces.Services;
using UserService.Application.Interfaces.Services.Security;
using UserService.Domain.Entities;

namespace UserService.Application.Features.Applications.Handlers;

public sealed class CreateApplicationCommandHandler : IRequestHandler<CreateApplicationCommand, CreateApplicationResponse>
{
    private readonly IApplicationAppService _applicationService;
    private readonly IAuditLogger _audit;

    public CreateApplicationCommandHandler(IApplicationAppService applicationService, IAuditLogger audit)
    {
        _applicationService = applicationService;
        _audit = audit;
    }

    public async Task<CreateApplicationResponse> Handle(CreateApplicationCommand command, CancellationToken cancellationToken)
    {
        var result = await _applicationService.CreateAsync(command, cancellationToken);
        await _audit.LogAsync(AuditEventTypes.ApplicationCreated, true, new AuditEntry(
            TargetType: "application", TargetId: result.Id.ToString(), ApplicationId: result.Id, Details: new { name = result.Name }), cancellationToken);
        return result;
    }
}
