using MediatR;

namespace UserService.Application.Features.Auth.Commands;

public record SsoAuthorizeCommand(
    string Email,
    string Password,
    string ClientId,
    string RedirectUri,
    string[] Scopes,
    string? State = null,
    string? CodeChallenge = null,
    string? CodeChallengeMethod = "S256"
) : IRequest<SsoAuthorizeResponse>;

public record SsoAuthorizeResponse(
    bool RequiresConsent,
    string? Code,
    string RedirectUri,
    string? State,
    string? ClientName = null,
    string[]? RequestedScopes = null,
    string[]? MissingScopes = null
);
