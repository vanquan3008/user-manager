using UserService.Domain.Entities;

namespace UserService.Application.Interfaces.Services.Security;

public sealed record IssuedSession(UserSession Session, string RefreshToken);

public interface ISessionService
{
    Task<IssuedSession> StartAsync(
        Guid userId,
        string sessionKind,
        string authenticationMethod,
        Guid? applicationId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Rotates the refresh token of a session. Presenting a superseded token outside the grace period
    /// is treated as token theft and revokes the session.
    /// </summary>
    Task<IssuedSession> RotateAsync(string refreshToken, CancellationToken cancellationToken = default);

    Task<UserSession?> GetAsync(Guid sessionId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserSession>> ListActiveAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> RevokeAsync(Guid sessionId, string reason, CancellationToken cancellationToken = default);
    Task<int> RevokeAllAsync(Guid userId, string reason, Guid? exceptSessionId = null, CancellationToken cancellationToken = default);
}

/// <summary>Used by the API's bearer-token validation to reject tokens of revoked sessions.</summary>
public interface ISessionValidator
{
    Task<bool> IsActiveAsync(Guid sessionId, CancellationToken cancellationToken = default);
}
