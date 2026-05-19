using MediatR;
using UserService.Application.DTOs.Auth;
using UserService.Application.Features.Auth.Commands;
using UserService.Application.Interfaces.Services;

namespace UserService.Application.Features.Auth.Handlers;

public class SSOExchangeTokenCommandHandler : IRequestHandler<SSOExchangeTokenCommand, AuthResponse>
{
    private readonly ISSOAuthenticationService _ssoAuthenticationService;

    public SSOExchangeTokenCommandHandler(ISSOAuthenticationService ssoAuthenticationService)
    {
        _ssoAuthenticationService = ssoAuthenticationService;
    }

    public Task<AuthResponse> Handle(SSOExchangeTokenCommand command, CancellationToken cancellationToken)
        => _ssoAuthenticationService.ExchangeTokenAsync(command, cancellationToken);
}
