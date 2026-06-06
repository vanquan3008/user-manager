using Microsoft.AspNetCore.Mvc;

namespace UserService.Api.Contracts.OAuth;

public sealed record OAuthAuthorizeQueryRequest(
    [property: FromQuery(Name = "client_id")] string? ClientId,
    [property: FromQuery(Name = "redirect_uri")] string? RedirectUri,
    [property: FromQuery(Name = "scope")] string? Scope = null,
    [property: FromQuery(Name = "state")] string? State = null,
    [property: FromQuery(Name = "code_challenge")] string? CodeChallenge = null,
    [property: FromQuery(Name = "code_challenge_method")] string? CodeChallengeMethod = "S256",
    [property: FromQuery(Name = "prompt")] string? Prompt = null,
    [property: FromQuery(Name = "scopes")] string[]? Scopes = null
);
