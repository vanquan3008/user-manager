using MediatR;
using UserService.Application.DTOs.Auth;
using UserService.Application.Features.Auth.Commands;
using UserService.Application.Interfaces.Services;

namespace UserService.Application.Features.Auth.Handlers;

public class VerifyMagicLinkCommandHandler : IRequestHandler<VerifyMagicLinkCommand, AuthResponse>
{
    private readonly IMagicLinkAuthService _magicLinkAuthService;
    private readonly IApplicationAppService _applicationService;

    public VerifyMagicLinkCommandHandler(IMagicLinkAuthService magicLinkAuthService, IApplicationAppService applicationService)
    {
        _magicLinkAuthService = magicLinkAuthService;
        _applicationService = applicationService;
    }

    public async Task<AuthResponse> Handle(VerifyMagicLinkCommand command, CancellationToken cancellationToken)
    {
        var applicationId = await _applicationService.ResolveApplicationIdAsync(command.ClientId, cancellationToken);

        var request = new VerifyMagicLinkRequest(command.Token, command.Purpose, applicationId, command.ClientId);
        return await _magicLinkAuthService.VerifyMagicLinkAndAuthenticateAsync(request, cancellationToken);
    }
}
