using MediatR;

namespace UserService.Application.Features.Auth.Commands;

public record SsoConsentCommand(
    string Email,
    string Password,
    string ClientId,
    string RedirectUri,
    string[] Scopes,
    bool Approve,
    string? State = null,
    string? CodeChallenge = null,
    string? CodeChallengeMethod = "S256"
) : IRequest<SsoAuthorizeResponse>;
