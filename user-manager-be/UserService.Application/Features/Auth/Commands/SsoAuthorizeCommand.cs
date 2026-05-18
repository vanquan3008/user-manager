using MediatR;

namespace UserService.Application.Features.Auth.Commands;

public record SSOAuthorizeCommand(
    string Email,
    string Password,
    string ClientId,
    string RedirectUri,
    string[] Scopes,
    string? State = null,
    string? CodeChallenge = null,
    string? CodeChallengeMethod = "S256"
) : IRequest<SSOAuthorizeResponse>;

public record SSOAuthorizeResponse(
    bool RequiresConsent,
    string? Code,
    string RedirectUri,
    string? State,
    string? ClientName = null,
    string[]? RequestedScopes = null,
    string[]? MissingScopes = null
);
