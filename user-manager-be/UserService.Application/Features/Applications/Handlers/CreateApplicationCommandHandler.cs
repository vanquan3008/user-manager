using MediatR;
using UserService.Application.DTOs.Applications;
using UserService.Application.Features.Applications.Commands;
using UserService.Application.Interfaces.Services;

namespace UserService.Application.Features.Applications.Handlers;

public sealed class CreateApplicationCommandHandler : IRequestHandler<CreateApplicationCommand, CreateApplicationResponse>
{
    private readonly IApplicationAppService _applicationService;

    public CreateApplicationCommandHandler(IApplicationAppService applicationService)
    {
        _applicationService = applicationService;
    }

    public Task<CreateApplicationResponse> Handle(CreateApplicationCommand command, CancellationToken cancellationToken)
        => _applicationService.CreateAsync(command, cancellationToken);
}
