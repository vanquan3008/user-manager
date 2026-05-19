using MediatR;
using UserService.Application.DTOs.Auth;
using UserService.Application.Features.Auth.Commands;
using UserService.Application.Interfaces.Services;

namespace UserService.Application.Features.Auth.Handlers;

public class SSOLoginCommandHandler : IRequestHandler<SSOLoginCommand, AuthResponse>
{
    private readonly ISSOAuthenticationService _ssoAuthenticationService;

    public SSOLoginCommandHandler(ISSOAuthenticationService ssoAuthenticationService)
    {
        _ssoAuthenticationService = ssoAuthenticationService;
    }

    public async Task<AuthResponse> Handle(SSOLoginCommand command, CancellationToken cancellationToken)
    {
        var request = new SSOLoginRequest(command.Email, command.Password);
        return await _ssoAuthenticationService.SSOLoginAsync(request, cancellationToken);
    }
}
