using UserService.Domain.Entities;

namespace UserService.Domain.Repositories;

public interface IOAuthRefreshTokenRepository : IRepository<OAuthRefreshToken>
{
    /// <summary>Returns the token by hash regardless of revocation state (needed for reuse detection).</summary>
    Task<OAuthRefreshToken?> GetByHashAsync(string tokenHash, CancellationToken cancellationToken = default);
    Task<int> RevokeAllForUserClientAsync(Guid userId, Guid applicationId, string reason, CancellationToken cancellationToken = default);
    /// <summary>Revokes all active grants of a session; returns the distinct applications affected.</summary>
    Task<IReadOnlyList<Guid>> RevokeAllForSessionAsync(Guid sessionId, string reason, CancellationToken cancellationToken = default);
    /// <summary>Revokes all active grants of a user; returns the distinct applications affected.</summary>
    Task<IReadOnlyList<Guid>> RevokeAllForUserAsync(Guid userId, string reason, CancellationToken cancellationToken = default);
    Task<int> DeleteExpiredAsync(DateTime olderThan, CancellationToken cancellationToken = default);
}
