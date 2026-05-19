using MediatR;
using UserService.Application.Features.Auth.Commands;
using UserService.Application.Interfaces.Services;

namespace UserService.Application.Features.Auth.Handlers;

public class SSOAuthorizeCommandHandler : IRequestHandler<SSOAuthorizeCommand, SSOAuthorizeResponse>
{
    private readonly ISSOAuthenticationService _ssoAuthenticationService;

    public SSOAuthorizeCommandHandler(ISSOAuthenticationService ssoAuthenticationService)
    {
        _ssoAuthenticationService = ssoAuthenticationService;
    }

    public Task<SSOAuthorizeResponse> Handle(SSOAuthorizeCommand command, CancellationToken cancellationToken)
        => _ssoAuthenticationService.AuthorizeAsync(command, cancellationToken);
}
