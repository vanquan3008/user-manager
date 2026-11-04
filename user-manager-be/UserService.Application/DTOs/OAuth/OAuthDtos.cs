namespace UserService.Application.DTOs.OAuth;

public record OAuthAuthorizeRequest(
    string ClientId,
    string RedirectUri,
    string? Scope = null,
    string? State = null,
    string? CodeChallenge = null,
    string? CodeChallengeMethod = "S256",
    string? Prompt = null,
    string[]? Scopes = null,
    string? Nonce = null,
    string? ResponseType = null
);


public record OAuthAuthorizeResponse(
    bool RequiresConsent,
    string ClientName,
    string RedirectUri,
    string[] RequestedScopes,
    string[] AlreadyGrantedScopes,
    string[] MissingScopes,
    string? Code,
    string? State
);

public record OAuthConsentRequest(
    string ClientId,
    string RedirectUri,
    string[] Scopes,
    string? State,
    string CodeChallenge,
    string CodeChallengeMethod = "S256",
    bool Approve = true,
    string? Nonce = null
);

public record OAuthTokenRequest(
    string GrantType,
    string ClientId,
    string RedirectUri,
    string? ClientSecret = null,
    string? Code = null,
    string? CodeVerifier = null,
    string? RefreshToken = null
);

public record OAuthUserDto(
    Guid Id,
    string Email,
    string UserName,
    string? DisplayName,
    IEnumerable<string> Roles
);

public record OAuthTokenResponse(
    string AccessToken,
    string TokenType,
    int ExpiresIn,
    string RefreshToken,
    string[] Scopes,
    OAuthUserDto User,
    string? IdToken = null
);

/// <summary>OIDC UserInfo response (claims filtered by granted scopes).</summary>
public record OAuthUserInfoDto(
    string Sub,
    string? Email,
    bool? EmailVerified,
    string? Name,
    string? PreferredUsername);
