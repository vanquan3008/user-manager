using MediatR;

namespace UserService.Application.Features.Auth.Commands;

/// <summary>
/// Command for SSO authorization - exchanges SSO token for application-specific token.
/// This is called after SSO login when user selects which application to access.
/// </summary>
public sealed record SsoAuthorizeCommand(
    Guid UserId,
    Guid ApplicationId,
    string? ReturnUrl = null
) : IRequest<SsoAuthorizeResponse>;

public sealed record SsoAuthorizeResponse(
    string AccessToken,
    string RefreshToken,
    string? RedirectUrl
);
