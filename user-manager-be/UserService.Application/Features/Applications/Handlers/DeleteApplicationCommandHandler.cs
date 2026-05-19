using MediatR;
using UserService.Application.Features.Applications.Commands;
using UserService.Application.Interfaces.Services;

namespace UserService.Application.Features.Applications.Handlers;

public sealed class DeleteApplicationCommandHandler : IRequestHandler<DeleteApplicationCommand, Unit>
{
    private readonly IApplicationAppService _applicationService;

    public DeleteApplicationCommandHandler(IApplicationAppService applicationService)
    {
        _applicationService = applicationService;
    }

    public async Task<Unit> Handle(DeleteApplicationCommand request, CancellationToken cancellationToken)
    {
        await _applicationService.DeleteAsync(request.ApplicationId, cancellationToken);
        return Unit.Value;
    }
}

