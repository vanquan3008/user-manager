using MediatR;
using UserService.Application.Features.Applications.Commands;
using UserService.Application.Interfaces.Services;
using UserService.Application.Interfaces.Services.Security;
using UserService.Domain.Entities;

namespace UserService.Application.Features.Applications.Handlers;

public sealed class DeleteApplicationCommandHandler : IRequestHandler<DeleteApplicationCommand, Unit>
{
    private readonly IApplicationAppService _applicationService;
    private readonly IAuditLogger _audit;

    public DeleteApplicationCommandHandler(IApplicationAppService applicationService, IAuditLogger audit)
    {
        _applicationService = applicationService;
        _audit = audit;
    }

    public async Task<Unit> Handle(DeleteApplicationCommand request, CancellationToken cancellationToken)
    {
        await _applicationService.DeleteAsync(request.ApplicationId, cancellationToken);
        await _audit.LogAsync(AuditEventTypes.ApplicationDeleted, true, new AuditEntry(
            TargetType: "application", TargetId: request.ApplicationId.ToString(), ApplicationId: request.ApplicationId), cancellationToken);
        return Unit.Value;
    }
}
