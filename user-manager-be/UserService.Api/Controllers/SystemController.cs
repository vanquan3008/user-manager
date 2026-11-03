using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using UserService.Api.Authorization;
using UserService.Application.Configuration;
using UserService.Application.Interfaces.Services.Security;
using UserService.Domain.Entities;

namespace UserService.Api.Controllers;

public sealed record SecuritySettingsDto(
    string Issuer,
    string Audience,
    int AccessTokenMinutes,
    int RefreshTokenIdleDays,
    int SessionAbsoluteDays,
    int RefreshReuseGraceSeconds,
    int MfaChallengeMinutes,
    int SigningKeyRotationDays,
    int RetiredKeyRetentionDays,
    int AuditRetentionDays,
    int RateLimitRequests,
    int RateLimitWindowSeconds,
    int AuthRateLimitRequests,
    int AuthRateLimitWindowSeconds,
    int PasswordMinLength,
    bool PasswordRequiresDigit,
    bool PasswordRequiresUppercase,
    bool PasswordRequiresLowercase,
    bool PasswordRequiresSymbol,
    int LockoutMaxFailedAttempts,
    int LockoutMinutes,
    string DiscoveryUrl,
    string JwksUrl);

/// <summary>Platform security administration: signing key lifecycle and effective security settings.</summary>
[ApiController]
[Route("api/system")]
[Produces("application/json")]
public sealed class SystemController : ApiControllerBase
{
    private readonly ISigningKeyProvider _keys;
    private readonly IAuditLogger _audit;
    private readonly AppSettings _settings;
    private readonly IdentityOptions _identityOptions;

    public SystemController(ISigningKeyProvider keys, IAuditLogger audit, AppSettings settings, IOptions<IdentityOptions> identityOptions)
    {
        _keys = keys;
        _audit = audit;
        _settings = settings;
        _identityOptions = identityOptions.Value;
    }

    [HttpGet("signing-keys")]
    [Authorize(Policy = AuthorizationPolicies.ViewSecurity)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<SigningKeyInfo>>>> GetSigningKeys(CancellationToken cancellationToken)
        => OkResponse(await _keys.ListAsync(cancellationToken));

    [HttpPost("signing-keys/rotate")]
    [Authorize(Policy = AuthorizationPolicies.ManageSecurity)]
    public async Task<ActionResult<ApiResponse<SigningKeyInfo>>> RotateSigningKey(CancellationToken cancellationToken)
    {
        var key = await _keys.RotateAsync(cancellationToken);
        await _audit.LogAsync(AuditEventTypes.SigningKeyRotated, true, new AuditEntry(TargetType: "signing_key", TargetId: key.KeyId), cancellationToken);
        return OkResponse(key, "Signing key rotated. The previous key remains published until its retention window ends.");
    }

    [HttpPost("signing-keys/{keyId}/revoke")]
    [Authorize(Policy = AuthorizationPolicies.ManageSecurity)]
    public async Task<ActionResult<ApiResponse<SuccessResponse>>> RevokeSigningKey(string keyId, CancellationToken cancellationToken)
    {
        await _keys.RevokeAsync(keyId, cancellationToken);
        await _audit.LogAsync(AuditEventTypes.SigningKeyRevoked, true, new AuditEntry(TargetType: "signing_key", TargetId: keyId), cancellationToken);
        return OkSuccess("Signing key revoked. Tokens signed with it are no longer accepted.");
    }

    [HttpGet("security-settings")]
    [Authorize(Policy = AuthorizationPolicies.ViewSecurity)]
    public ActionResult<ApiResponse<SecuritySettingsDto>> GetSecuritySettings()
    {
        var api = Uri.TryCreate(_settings.ApiPublicBaseUrl, UriKind.Absolute, out var u)
            ? u.ToString().TrimEnd('/')
            : $"{Request.Scheme}://{Request.Host}";
        var s = _settings.Security;
        var p = _identityOptions.Password;
        return OkResponse(new SecuritySettingsDto(
            _settings.Jwt.Issuer,
            _settings.Jwt.Audience,
            _settings.Jwt.AccessTokenExpirationMinutes,
            _settings.Jwt.RefreshTokenExpirationDays,
            s.SessionAbsoluteLifetimeDays,
            s.RefreshTokenReuseGraceSeconds,
            s.MfaChallengeMinutes,
            s.SigningKeys.RotationDays,
            s.SigningKeys.RetiredKeyRetentionDays,
            s.AuditLogRetentionDays,
            _settings.RateLimit.RequestLimit,
            _settings.RateLimit.WindowSeconds,
            _settings.RateLimit.AuthRequestLimit,
            _settings.RateLimit.AuthWindowSeconds,
            p.RequiredLength,
            p.RequireDigit,
            p.RequireUppercase,
            p.RequireLowercase,
            p.RequireNonAlphanumeric,
            _identityOptions.Lockout.MaxFailedAccessAttempts,
            (int)_identityOptions.Lockout.DefaultLockoutTimeSpan.TotalMinutes,
            $"{api}/.well-known/openid-configuration",
            $"{api}/.well-known/jwks.json"));
    }
}
