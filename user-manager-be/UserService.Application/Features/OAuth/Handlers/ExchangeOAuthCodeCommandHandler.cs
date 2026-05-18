using MediatR;
using UserService.Application.Features.OAuth.Commands;
using UserService.Application.Interfaces.Services;
using UserService.Application.DTOs.OAuth;

namespace UserService.Application.Features.OAuth.Handlers;

public sealed class ExchangeOAuthCodeCommandHandler : IRequestHandler<ExchangeOAuthCodeCommand, OAuthTokenResponse>
{
    private readonly IOAuthFlowService _oauthFlowService;

    public ExchangeOAuthCodeCommandHandler(IOAuthFlowService oauthFlowService)
    {
        _oauthFlowService = oauthFlowService;
    }

    public Task<OAuthTokenResponse> Handle(ExchangeOAuthCodeCommand command, CancellationToken cancellationToken)
        => _oauthFlowService.ExchangeTokenAsync(command.Request, cancellationToken);
}

