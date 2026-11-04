namespace UserService.Application.DTOs.Auth;

/// <summary>
/// Immutable request object for generating an access token.
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
    IEnumerable<string>? OAuthScopes = null,
    Guid? SessionId = null,
    string? ClientId = null,
    string? AuthenticationMethod = null
);

/// <summary>Inputs for an OpenID Connect ID token (OIDC Core §2).</summary>
public sealed record IdTokenRequest(
    Guid UserId,
    string ClientId,
    string? Nonce,
    Guid? SessionId,
    DateTime AuthTime,
    IReadOnlyCollection<string> Scopes,
    string? Email,
    bool EmailVerified,
    string? Name,
    string? PreferredUsername,
    string? AuthenticationMethod);
