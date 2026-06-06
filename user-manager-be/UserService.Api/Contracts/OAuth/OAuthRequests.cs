namespace UserService.Api.Contracts.OAuth;

public sealed record OAuthConsentApiRequest(
    string ClientId,
    string RedirectUri,
    string[] Scopes,
    string? State,
    string CodeChallenge,
    string CodeChallengeMethod = "S256",
    bool Approve = true
);

public sealed record OAuthTokenApiRequest(
    string GrantType,
    string ClientId,
    string RedirectUri,
    string? ClientSecret = null,
    string? Code = null,
    string? CodeVerifier = null,
    string? RefreshToken = null
);
