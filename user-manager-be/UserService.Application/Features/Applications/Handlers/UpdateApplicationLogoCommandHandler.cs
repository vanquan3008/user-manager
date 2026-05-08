using MediatR;
using UserService.Application.DTOs.Applications;
using UserService.Application.Features.Applications.Commands;
using UserService.Application.Interfaces.Services;
using UserService.Application.Mapping;
using UserService.Application.Services;
using UserService.Domain.Repositories;

namespace UserService.Application.Features.Applications.Handlers;

public sealed class UpdateApplicationLogoCommandHandler
    : IRequestHandler<UpdateApplicationLogoCommand, CreateApplicationResponse>
{
    private readonly IApplicationAppService _applicationService;
    private readonly IUserRepository _users;

    public UpdateApplicationLogoCommandHandler(IApplicationAppService applicationService, IUserRepository users)
    {
        _applicationService = applicationService;
        _users = users;
    }

    public async Task<CreateApplicationResponse> Handle(UpdateApplicationLogoCommand request, CancellationToken cancellationToken)
    {
        var app = await _applicationService.UpdateLogoUrlAsync(request.ApplicationId, request.LogoUrl, cancellationToken);
        if (app is null)
            throw new KeyNotFoundException("Application not found.");

        var owner = await _users.GetByIdAsync(app.OwnerId, cancellationToken);
        return ApplicationEntityMapper.ToCreateApplicationResponse(app, owner);
    }
}
