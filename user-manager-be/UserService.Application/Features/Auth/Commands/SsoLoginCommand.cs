using MediatR;
using UserService.Application.Common;

namespace UserService.Application.Features.Auth.Commands;

/// <summary>
/// Command for SSO user login.
/// Authenticates the SSO account and creates an auth-server session.
/// </summary>
public sealed record SsoLoginCommand(
    string Email,
    string Password
) : IRequest<SsoLoginResponse>;

public sealed record SsoLoginResponse(
    string AccessToken,
    string? RefreshToken,
    SsoUserDto User,
    bool MfaRequired = false,
    string? MfaToken = null,
    DateTime? MfaExpiresAt = null,
    Guid? SessionId = null
);

public sealed record SsoUserDto(
    Guid UserId,
    string Email,
    string Username,
    string DisplayName,
    IReadOnlyList<string> Roles,
    IReadOnlyList<string> Permissions
);
