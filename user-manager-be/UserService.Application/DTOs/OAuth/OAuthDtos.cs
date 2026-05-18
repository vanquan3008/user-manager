using System.Text.Json.Serialization;

namespace UserService.Application.DTOs.OAuth;

public record OAuthAuthorizeRequest(
    string ClientId,
    string RedirectUri,
    string? Scope = null,
    string? State = null,
    string? CodeChallenge = null,
    string? CodeChallengeMethod = "S256",
    string? Prompt = null,
    string[]? Scopes = null
);


public record OAuthAuthorizeResponse(
    [property: JsonPropertyName("requiresConsent")] bool RequiresConsent,
    [property: JsonPropertyName("clientName")] string ClientName,
    [property: JsonPropertyName("redirectUri")] string RedirectUri,
    [property: JsonPropertyName("requestedScopes")] string[] RequestedScopes,
    [property: JsonPropertyName("alreadyGrantedScopes")] string[] AlreadyGrantedScopes,
    [property: JsonPropertyName("missingScopes")] string[] MissingScopes,
    [property: JsonPropertyName("code")] string? Code,
    [property: JsonPropertyName("state")] string? State
);

public record OAuthConsentRequest(
    [property: JsonPropertyName("clientId")] string ClientId,
    [property: JsonPropertyName("redirectUri")] string RedirectUri,
    [property: JsonPropertyName("scopes")] string[] Scopes,
    [property: JsonPropertyName("state")] string? State,
    [property: JsonPropertyName("codeChallenge")] string CodeChallenge,
    [property: JsonPropertyName("codeChallengeMethod")] string CodeChallengeMethod = "S256",
    [property: JsonPropertyName("approve")] bool Approve = true
);

public record OAuthTokenRequest(
    [property: JsonPropertyName("grantType")] string GrantType,
    [property: JsonPropertyName("clientId")] string ClientId,
    [property: JsonPropertyName("redirectUri")] string RedirectUri,
    [property: JsonPropertyName("clientSecret")] string? ClientSecret = null,
    [property: JsonPropertyName("code")] string? Code = null,
    [property: JsonPropertyName("codeVerifier")] string? CodeVerifier = null,
    [property: JsonPropertyName("refreshToken")] string? RefreshToken = null
);

public record OAuthUserDto(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("email")] string Email,
    [property: JsonPropertyName("userName")] string UserName,
    [property: JsonPropertyName("displayName")] string? DisplayName,
    [property: JsonPropertyName("roles")] IEnumerable<string> Roles
);

public record OAuthTokenResponse(
    [property: JsonPropertyName("accessToken")] string AccessToken,
    [property: JsonPropertyName("tokenType")] string TokenType,
    [property: JsonPropertyName("expiresIn")] int ExpiresIn,
    [property: JsonPropertyName("refreshToken")] string RefreshToken,
    [property: JsonPropertyName("scopes")] string[] Scopes,
    [property: JsonPropertyName("user")] OAuthUserDto User
);
