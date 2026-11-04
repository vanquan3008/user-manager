using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using UserService.Api.Contracts.Account;
using UserService.Api.Extensions;
using UserService.Api.Security;
using UserService.Application.Interfaces.Services.Security;
using UserService.Domain.Entities;
using UserService.Domain.Repositories;

namespace UserService.Api.Controllers;

/// <summary>Self-service security for the signed-in user: sessions/devices, MFA and sign-in history.</summary>
[ApiController]
[Route("api/account")]
[Authorize]
[Produces("application/json")]
public sealed class AccountController : ApiControllerBase
{
    private static readonly string[] ActivityCategories = { "auth", "session", "mfa", "oauth" };

    private readonly ISessionService _sessions;
    private readonly IMfaService _mfa;
    private readonly IAuditLogger _audit;
    private readonly IAuditLogRepository _auditLogs;

    public AccountController(ISessionService sessions, IMfaService mfa, IAuditLogger audit, IAuditLogRepository auditLogs)
    {
        _sessions = sessions;
        _mfa = mfa;
        _audit = audit;
        _auditLogs = auditLogs;
    }

    [HttpGet("sessions")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<SessionDto>>>> GetSessions(CancellationToken cancellationToken)
    {
        var current = User.TryGetSessionId();
        var sessions = await _sessions.ListActiveAsync(User.GetRequiredCurrentUserId(), cancellationToken);
        return OkResponse<IReadOnlyList<SessionDto>>(sessions.Select(s => ToDto(s, current)).ToList());
    }

    [HttpDelete("sessions/{sessionId:guid}")]
    public async Task<ActionResult<ApiResponse<SuccessResponse>>> RevokeSession(Guid sessionId, CancellationToken cancellationToken)
    {
        var userId = User.GetRequiredCurrentUserId();
        var session = await _sessions.GetAsync(sessionId, cancellationToken);
        if (session is null || session.UserId != userId)
            return NotFoundResponse("Session not found.");

        await _sessions.RevokeAsync(sessionId, SessionRevocationReasons.UserRevoked, cancellationToken);
        await _audit.LogAsync(AuditEventTypes.SessionRevoked, true, new AuditEntry(TargetType: "session", TargetId: sessionId.ToString(), SessionId: sessionId), cancellationToken);
        return OkSuccess("Session signed out.");
    }

    [HttpPost("sessions/revoke-others")]
    public async Task<ActionResult<ApiResponse<SuccessResponse>>> RevokeOtherSessions(CancellationToken cancellationToken)
    {
        var count = await _sessions.RevokeAllAsync(User.GetRequiredCurrentUserId(), SessionRevocationReasons.UserRevoked, User.TryGetSessionId(), cancellationToken);
        await _audit.LogAsync(AuditEventTypes.SessionsRevokedAll, true, new AuditEntry(Details: new { scope = "others", sessions = count }), cancellationToken);
        return OkSuccess($"Signed out of {count} other session(s).");
    }

    [HttpGet("mfa")]
    public async Task<ActionResult<ApiResponse<MfaStatus>>> GetMfaStatus(CancellationToken cancellationToken)
        => OkResponse(await _mfa.GetStatusAsync(User.GetRequiredCurrentUserId(), cancellationToken));

    [HttpPost("mfa/setup")]
    [EnableRateLimiting(RateLimitPolicies.Auth)]
    public async Task<ActionResult<ApiResponse<MfaSetupDto>>> BeginMfaSetup(CancellationToken cancellationToken)
    {
        var setup = await _mfa.BeginSetupAsync(User.GetRequiredCurrentUserId(), cancellationToken);
        Response.Headers.CacheControl = "no-store";
        return OkResponse(new MfaSetupDto(setup.SharedKey, setup.AuthenticatorUri));
    }

    [HttpPost("mfa/enable")]
    [EnableRateLimiting(RateLimitPolicies.Auth)]
    public async Task<ActionResult<ApiResponse<RecoveryCodesDto>>> EnableMfa([FromBody] MfaCodeApiRequest request, CancellationToken cancellationToken)
    {
        var userId = User.GetRequiredCurrentUserId();
        var codes = await _mfa.EnableAsync(userId, request.Code, cancellationToken);
        // A new factor was added: sign out other devices so they re-authenticate with it.
        await _sessions.RevokeAllAsync(userId, SessionRevocationReasons.MfaChanged, User.TryGetSessionId(), cancellationToken);
        await _audit.LogAsync(AuditEventTypes.MfaEnabled, true, cancellationToken: cancellationToken);
        Response.Headers.CacheControl = "no-store";
        return OkResponse(new RecoveryCodesDto(codes));
    }

    [HttpPost("mfa/disable")]
    [EnableRateLimiting(RateLimitPolicies.Auth)]
    public async Task<ActionResult<ApiResponse<SuccessResponse>>> DisableMfa([FromBody] MfaDisableApiRequest request, CancellationToken cancellationToken)
    {
        var userId = User.GetRequiredCurrentUserId();
        await _mfa.DisableAsync(userId, request.Password, request.Code, cancellationToken);
        await _sessions.RevokeAllAsync(userId, SessionRevocationReasons.MfaChanged, User.TryGetSessionId(), cancellationToken);
        await _audit.LogAsync(AuditEventTypes.MfaDisabled, true, cancellationToken: cancellationToken);
        return OkSuccess("Multi-factor authentication disabled.");
    }

    [HttpPost("mfa/recovery-codes")]
    [EnableRateLimiting(RateLimitPolicies.Auth)]
    public async Task<ActionResult<ApiResponse<RecoveryCodesDto>>> RegenerateRecoveryCodes([FromBody] MfaCodeApiRequest request, CancellationToken cancellationToken)
    {
        var codes = await _mfa.RegenerateRecoveryCodesAsync(User.GetRequiredCurrentUserId(), request.Code, cancellationToken);
        await _audit.LogAsync(AuditEventTypes.MfaRecoveryCodesRegenerated, true, cancellationToken: cancellationToken);
        Response.Headers.CacheControl = "no-store";
        return OkResponse(new RecoveryCodesDto(codes));
    }

    /// <summary>The caller's own authentication history.</summary>
    [HttpGet("activity")]
    public async Task<ActionResult<PagedResponse<AuditLogDto>>> GetActivity(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var userId = User.GetRequiredCurrentUserId();
        pageSize = Math.Clamp(pageSize, 1, 100);
        var (items, total) = await _auditLogs.QueryAsync(
            new AuditLogQuery(page, pageSize, UserId: userId, Categories: ActivityCategories), cancellationToken);
        return Ok(new PagedResponse<AuditLogDto>(true, items.Select(AuditLogMapping.ToDto).ToList(), total, page, pageSize));
    }

    internal static SessionDto ToDto(UserSession s, Guid? current) => new(
        s.Id, s.SessionKind, s.AuthenticationMethod, s.IpAddress, s.UserAgent,
        s.CreatedAt, s.LastSeenAt, s.ExpiresAt, s.Id == current);
}

internal static class AuditLogMapping
{
    public static AuditLogDto ToDto(AuditLog x) => new(
        x.Id, x.OccurredAt, x.EventType, x.Category, x.Succeeded, x.ActorUserId, x.ActorEmail,
        x.TargetUserId, x.TargetType, x.TargetId, x.ApplicationId, x.SessionId, x.IpAddress,
        x.UserAgent, x.CorrelationId, x.Reason, x.Details);
}
