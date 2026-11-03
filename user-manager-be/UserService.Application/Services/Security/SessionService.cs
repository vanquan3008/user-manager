using Microsoft.Extensions.Caching.Memory;
using UserService.Application.Common;
using UserService.Application.Configuration;
using UserService.Application.Interfaces.Services;
using UserService.Application.Interfaces.Services.Security;
using UserService.Domain.Entities;
using UserService.Domain.Exceptions;
using UserService.Domain.Interfaces;
using UserService.Domain.Repositories;

namespace UserService.Application.Services.Security;

public sealed class SessionService : ISessionService, ISessionValidator
{
    private readonly IUserSessionRepository _sessions;
    private readonly IOAuthRefreshTokenRepository _oauthRefreshTokens;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClock _clock;
    private readonly AppSettings _settings;
    private readonly IMemoryCache _cache;
    private readonly IRequestContext _requestContext;
    private readonly IAuditLogger _audit;
    private readonly IBackchannelLogoutService _backchannel;

    public SessionService(
        IUserSessionRepository sessions,
        IOAuthRefreshTokenRepository oauthRefreshTokens,
        IUnitOfWork unitOfWork,
        IClock clock,
        AppSettings settings,
        IMemoryCache cache,
        IRequestContext requestContext,
        IAuditLogger audit,
        IBackchannelLogoutService backchannel)
    {
        _sessions = sessions;
        _oauthRefreshTokens = oauthRefreshTokens;
        _unitOfWork = unitOfWork;
        _clock = clock;
        _settings = settings;
        _cache = cache;
        _requestContext = requestContext;
        _audit = audit;
        _backchannel = backchannel;
    }

    private TimeSpan IdleLifetime => TimeSpan.FromDays(Math.Max(1, _settings.Jwt.RefreshTokenExpirationDays));
    private TimeSpan AbsoluteLifetime => TimeSpan.FromDays(Math.Max(_settings.Jwt.RefreshTokenExpirationDays, _settings.Security.SessionAbsoluteLifetimeDays));

    public async Task<IssuedSession> StartAsync(
        Guid userId,
        string sessionKind,
        string authenticationMethod,
        Guid? applicationId = null,
        CancellationToken cancellationToken = default)
    {
        var refreshToken = SecureTokens.Create();
        var session = UserSession.Start(
            userId,
            SecureTokens.Hash(refreshToken),
            sessionKind,
            authenticationMethod,
            _clock.UtcNow,
            IdleLifetime,
            AbsoluteLifetime,
            _requestContext.IpAddress,
            _requestContext.UserAgent,
            applicationId);

        await _sessions.AddAsync(session, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return new IssuedSession(session, refreshToken);
    }

    public async Task<IssuedSession> RotateAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            throw new AuthenticationFlowException(AuthErrorCodes.InvalidRefreshToken, "Invalid refresh token.");

        var now = _clock.UtcNow;
        var hash = SecureTokens.Hash(refreshToken.Trim());
        var session = await _sessions.GetByRefreshTokenHashAsync(hash, cancellationToken);

        if (session is not null)
        {
            if (!session.IsActive(now))
                throw new AuthenticationFlowException(AuthErrorCodes.SessionExpired, "Your session has expired. Please sign in again.");

            var newToken = SecureTokens.Create();
            session.Rotate(SecureTokens.Hash(newToken), now, IdleLifetime, _requestContext.IpAddress, _requestContext.UserAgent);
            try
            {
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
            catch (ConcurrencyConflictException)
            {
                // Another request rotated the same token first; the client should retry with the new cookie.
                throw new AuthenticationFlowException(AuthErrorCodes.RefreshTokenSuperseded, "Refresh token was already rotated.");
            }

            _cache.Remove(CacheKey(session.Id));
            return new IssuedSession(session, newToken);
        }

        var superseded = await _sessions.GetByPreviousRefreshTokenHashAsync(hash, cancellationToken);
        if (superseded is not null && superseded.IsActive(now))
        {
            if (superseded.IsWithinRotationGracePeriod(now, TimeSpan.FromSeconds(_settings.Security.RefreshTokenReuseGraceSeconds)))
                throw new AuthenticationFlowException(AuthErrorCodes.RefreshTokenSuperseded, "Refresh token was already rotated.");

            await RevokeInternalAsync(superseded, SessionRevocationReasons.RefreshTokenReuse, cancellationToken);
            await _audit.LogAsync(AuditEventTypes.RefreshTokenReuseDetected, false, new AuditEntry(
                ActorUserId: superseded.UserId,
                SessionId: superseded.Id,
                Reason: "A superseded refresh token was replayed; the session was revoked."), cancellationToken);

            throw new AuthenticationFlowException(AuthErrorCodes.RefreshTokenReused, "Refresh token reuse detected. The session has been revoked.");
        }

        throw new AuthenticationFlowException(AuthErrorCodes.InvalidRefreshToken, "Invalid or expired refresh token.");
    }

    public Task<UserSession?> GetAsync(Guid sessionId, CancellationToken cancellationToken = default)
        => _sessions.GetByIdAsync(sessionId, cancellationToken);

    public Task<IReadOnlyList<UserSession>> ListActiveAsync(Guid userId, CancellationToken cancellationToken = default)
        => _sessions.GetActiveForUserAsync(userId, _clock.UtcNow, cancellationToken);

    public async Task<bool> RevokeAsync(Guid sessionId, string reason, CancellationToken cancellationToken = default)
    {
        var session = await _sessions.GetByIdAsync(sessionId, cancellationToken);
        if (session is null || session.IsRevoked)
            return false;

        await RevokeInternalAsync(session, reason, cancellationToken);
        return true;
    }

    public async Task<int> RevokeAllAsync(Guid userId, string reason, Guid? exceptSessionId = null, CancellationToken cancellationToken = default)
    {
        var sessions = await _sessions.GetActiveForUserAsync(userId, _clock.UtcNow, cancellationToken);
        var count = 0;
        foreach (var session in sessions.Where(s => s.Id != exceptSessionId))
        {
            await RevokeInternalAsync(session, reason, cancellationToken);
            count++;
        }
        return count;
    }

    public async Task<bool> IsActiveAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        var ttl = TimeSpan.FromSeconds(Math.Max(0, _settings.Security.SessionValidationCacheSeconds));
        if (ttl > TimeSpan.Zero && _cache.TryGetValue(CacheKey(sessionId), out bool cached))
            return cached;

        var session = await _sessions.GetByIdAsync(sessionId, cancellationToken);
        var active = session is not null && session.IsActive(_clock.UtcNow);
        if (ttl > TimeSpan.Zero)
            _cache.Set(CacheKey(sessionId), active, ttl);
        return active;
    }

    /// <summary>
    /// Revokes the session, every OAuth grant issued under it, and notifies the affected relying
    /// parties through back-channel logout (single logout).
    /// </summary>
    private async Task RevokeInternalAsync(UserSession session, string reason, CancellationToken cancellationToken)
    {
        session.Revoke(reason, _clock.UtcNow);
        var affectedApps = await _oauthRefreshTokens.RevokeAllForSessionAsync(session.Id, "session_" + reason, cancellationToken);
        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (ConcurrencyConflictException)
        {
            // Revoked or rotated concurrently; re-read and retry once so revocation always wins.
            var fresh = await _sessions.GetByIdAsync(session.Id, cancellationToken);
            if (fresh is not null && fresh.Revoke(reason, _clock.UtcNow))
                await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        _cache.Remove(CacheKey(session.Id));

        if (affectedApps.Count > 0)
            await _backchannel.NotifyApplicationsAsync(session.UserId, session.Id, affectedApps, cancellationToken);
    }

    private static string CacheKey(Guid sessionId) => $"session-active:{sessionId:N}";
}
