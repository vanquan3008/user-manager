namespace UserService.Application.DTOs.Auth;

/// <summary>
/// Immutable request object for generating an access token.
/// Eliminates primitive parameter explosion in ITokenService and IClaimsFactory.
/// </summary>
public sealed record AccessTokenRequest(
    Guid UserId,
    string Email,
    string? DisplayName,
    IEnumerable<string> Roles,
    IEnumerable<string>? Permissions = null,
    Guid? ApplicationId = null,
    string? Scope = null,
    IEnumerable<Guid>? SSOAppIds = null,
    IEnumerable<string>? OAuthScopes = null
);
