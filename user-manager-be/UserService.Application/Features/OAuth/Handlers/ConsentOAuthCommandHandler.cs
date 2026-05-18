using MediatR;
using UserService.Application.Interfaces.Services;
using UserService.Application.Features.OAuth.Commands;

namespace UserService.Application.Features.OAuth.Handlers;

public sealed class ConsentOAuthCommandHandler : IRequestHandler<ConsentOAuthCommand, OAuthAuthorizeResponse>
{
    private readonly IOAuthFlowService _oauthFlowService;

    public ConsentOAuthCommandHandler(IOAuthFlowService oauthFlowService)
    {
        _oauthFlowService = oauthFlowService;
    }

    public Task<OAuthAuthorizeResponse> Handle(ConsentOAuthCommand command, CancellationToken cancellationToken)
        => _oauthFlowService.ConsentAsync(command.UserId, command.Request, cancellationToken);
}
