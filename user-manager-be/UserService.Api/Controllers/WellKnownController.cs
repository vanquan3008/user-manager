using Microsoft.AspNetCore.Mvc;
using UserService.Application.Configuration;
using UserService.Application.Interfaces.Services.Security;

namespace UserService.Api.Controllers;

/// <summary>
/// Discovery metadata (OpenID Connect Discovery 1.0 / RFC 8414) and the JSON Web Key Set used to
/// verify every token issued by this server.
/// </summary>
[ApiController]
[Produces("application/json")]
public sealed class WellKnownController : ControllerBase
{
    private readonly ISigningKeyProvider _keys;
    private readonly AppSettings _settings;

    public WellKnownController(ISigningKeyProvider keys, AppSettings settings)
    {
        _keys = keys;
        _settings = settings;
    }

    [HttpGet("/.well-known/jwks.json")]
    [ResponseCache(Duration = 300)]
    public IActionResult Jwks() => Ok(new { keys = _keys.GetPublicJwks() });

    [HttpGet("/.well-known/openid-configuration")]
    [HttpGet("/.well-known/oauth-authorization-server")]
    [ResponseCache(Duration = 300)]
    public IActionResult Discovery()
    {
        var api = PublicApiBase();
        var frontend = (_settings.FrontendBaseUrl ?? string.Empty).TrimEnd('/');

        return Ok(new Dictionary<string, object>
        {
            ["issuer"] = _settings.Jwt.Issuer,
            // Interactive authorization is served by the hosted sign-in UI (login, MFA, consent).
            ["authorization_endpoint"] = $"{frontend}/oauth/authorize",
            ["token_endpoint"] = $"{api}/api/oauth/token",
            ["userinfo_endpoint"] = $"{api}/api/oauth/userinfo",
            ["revocation_endpoint"] = $"{api}/api/oauth/revoke",
            ["jwks_uri"] = $"{api}/.well-known/jwks.json",
            ["response_types_supported"] = new[] { "code" },
            ["response_modes_supported"] = new[] { "query" },
            ["grant_types_supported"] = new[] { "authorization_code", "refresh_token" },
            ["subject_types_supported"] = new[] { "public" },
            ["id_token_signing_alg_values_supported"] = new[] { "RS256" },
            ["token_endpoint_auth_methods_supported"] = new[] { "none", "client_secret_post", "client_secret_basic" },
            ["revocation_endpoint_auth_methods_supported"] = new[] { "none", "client_secret_post", "client_secret_basic" },
            ["code_challenge_methods_supported"] = new[] { "S256" },
            ["scopes_supported"] = new[] { "openid", "profile", "email" },
            ["claims_supported"] = new[] { "sub", "iss", "aud", "exp", "iat", "auth_time", "nonce", "sid", "amr", "email", "email_verified", "name", "preferred_username" },
            ["backchannel_logout_supported"] = true,
            ["backchannel_logout_session_supported"] = true
        });
    }

    private string PublicApiBase()
    {
        if (Uri.TryCreate(_settings.ApiPublicBaseUrl, UriKind.Absolute, out var configured))
            return configured.ToString().TrimEnd('/');
        return $"{Request.Scheme}://{Request.Host}{Request.PathBase}".TrimEnd('/');
    }
}
