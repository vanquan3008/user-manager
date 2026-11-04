namespace UserService.Api.Contracts.OAuth;

public sealed record OAuthConsentApiRequest(
    string ClientId,
    string RedirectUri,
    string[] Scopes,
    string? State,
    string CodeChallenge,
    string CodeChallengeMethod = "S256",
    bool Approve = true,
    string? Nonce = null
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

/// <summary>RFC 6749 form parameters (application/x-www-form-urlencoded).</summary>
public sealed class OAuthTokenFormRequest
{
    [Microsoft.AspNetCore.Mvc.FromForm(Name = "grant_type")] public string? GrantType { get; set; }
    [Microsoft.AspNetCore.Mvc.FromForm(Name = "client_id")] public string? ClientId { get; set; }
    [Microsoft.AspNetCore.Mvc.FromForm(Name = "client_secret")] public string? ClientSecret { get; set; }
    [Microsoft.AspNetCore.Mvc.FromForm(Name = "redirect_uri")] public string? RedirectUri { get; set; }
    [Microsoft.AspNetCore.Mvc.FromForm(Name = "code")] public string? Code { get; set; }
    [Microsoft.AspNetCore.Mvc.FromForm(Name = "code_verifier")] public string? CodeVerifier { get; set; }
    [Microsoft.AspNetCore.Mvc.FromForm(Name = "refresh_token")] public string? RefreshToken { get; set; }
}

public sealed class OAuthRevokeFormRequest
{
    [Microsoft.AspNetCore.Mvc.FromForm(Name = "token")] public string? Token { get; set; }
    [Microsoft.AspNetCore.Mvc.FromForm(Name = "token_type_hint")] public string? TokenTypeHint { get; set; }
    [Microsoft.AspNetCore.Mvc.FromForm(Name = "client_id")] public string? ClientId { get; set; }
    [Microsoft.AspNetCore.Mvc.FromForm(Name = "client_secret")] public string? ClientSecret { get; set; }
}
