namespace UserService.Application.DTOs.Auth;

/// <summary>
/// Immutable request object for SSO login.
/// </summary>
public sealed record SSOLoginRequest(
    string Email,
    string Password
);
