using MediatR;
using UserService.Application.Features.Auth.Commands;
using UserService.Application.Interfaces.Services;

namespace UserService.Application.Features.Auth.Handlers;

public class SSOConsentCommandHandler : IRequestHandler<SSOConsentCommand, SSOAuthorizeResponse>
{
    private readonly ISSOAuthenticationService _ssoAuthenticationService;

    public SSOConsentCommandHandler(ISSOAuthenticationService ssoAuthenticationService)
    {
        _ssoAuthenticationService = ssoAuthenticationService;
    }

    public Task<SSOAuthorizeResponse> Handle(SSOConsentCommand command, CancellationToken cancellationToken)
        => _ssoAuthenticationService.ConsentAsync(command, cancellationToken);
}
