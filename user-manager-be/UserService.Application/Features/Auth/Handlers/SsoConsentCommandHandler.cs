using MediatR;
using UserService.Application.Features.Auth.Commands;
using UserService.Application.DTOs.OAuth;
using UserService.Application.Features.OAuth.Commands;
using UserService.Application.Services;
using UserService.Domain.Repositories;

namespace UserService.Application.Features.Auth.Handlers;

public class SsoConsentCommandHandler : IRequestHandler<SsoConsentCommand, SsoAuthorizeResponse>
{
    private readonly IIdentityService _identityService;
    private readonly IUserRepository _userRepository;
    private readonly IMediator _mediator;

    public SsoConsentCommandHandler(
        IIdentityService identityService,
        IUserRepository userRepository,
        IMediator mediator)
    {
        _identityService = identityService;
        _userRepository = userRepository;
        _mediator = mediator;
    }

    public async Task<SsoAuthorizeResponse> Handle(SsoConsentCommand command, CancellationToken cancellationToken)
    {
        var (succeeded, appUser, errors) = await _identityService.LoginAsync(command.Email, command.Password, cancellationToken);
        if (!succeeded || appUser == null)
            throw new UnauthorizedAccessException(string.Join(", ", errors));

        var domainUser = await _userRepository.GetByIdAsync(appUser.Id, cancellationToken);
        if (domainUser == null)
            throw new InvalidOperationException("User profile not found");

        if (!domainUser.IsSSOAccount)
            throw new UnauthorizedAccessException("This account is not SSO enabled.");

        var oauthRequest = new OAuthConsentRequest(
            ClientId: command.ClientId,
            RedirectUri: command.RedirectUri,
            Scopes: command.Scopes,
            State: command.State,
            CodeChallenge: command.CodeChallenge ?? string.Empty,
            CodeChallengeMethod: command.CodeChallengeMethod ?? "S256",
            Approve: command.Approve);

        var response = await _mediator.Send(new ConsentOAuthCommand(appUser.Id, oauthRequest), cancellationToken);

        return new SsoAuthorizeResponse(
            RequiresConsent: response.RequiresConsent,
            Code: response.Code,
            RedirectUri: response.RedirectUri,
            State: response.State,
            ClientName: response.ClientName,
            RequestedScopes: response.RequestedScopes,
            MissingScopes: response.MissingScopes
        );
    }
}
