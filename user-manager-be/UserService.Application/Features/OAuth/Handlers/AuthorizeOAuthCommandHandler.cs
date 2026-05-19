using MediatR;
using UserService.Application.Features.OAuth.Commands;
using UserService.Application.DTOs.OAuth;
using UserService.Application.Interfaces.Services;

namespace UserService.Application.Features.OAuth.Handlers;

public sealed class AuthorizeOAuthCommandHandler : IRequestHandler<AuthorizeOAuthCommand, OAuthAuthorizeResponse>
{
    private readonly IOAuthFlowService _oauthFlowService;

    public AuthorizeOAuthCommandHandler(IOAuthFlowService oauthFlowService)
    {
        _oauthFlowService = oauthFlowService;
    }

    public Task<OAuthAuthorizeResponse> Handle(AuthorizeOAuthCommand command, CancellationToken cancellationToken)
        => _oauthFlowService.AuthorizeAsync(command.UserId, command.Request, cancellationToken);
}
