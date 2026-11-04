using UserService.Domain.Entities;

namespace UserService.Domain.Repositories;

public interface IUserSessionRepository
{
    Task AddAsync(UserSession session, CancellationToken cancellationToken = default);
    Task<UserSession?> GetByIdAsync(Guid sessionId, CancellationToken cancellationToken = default);
    Task<UserSession?> GetByRefreshTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default);
    Task<UserSession?> GetByPreviousRefreshTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserSession>> GetActiveForUserAsync(Guid userId, DateTime utcNow, CancellationToken cancellationToken = default);
    Task<int> CountActiveAsync(DateTime utcNow, CancellationToken cancellationToken = default);
    Task<int> DeleteExpiredAsync(DateTime olderThan, CancellationToken cancellationToken = default);
}
