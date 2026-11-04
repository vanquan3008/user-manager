using Microsoft.EntityFrameworkCore;
using UserService.Domain.Entities;
using UserService.Domain.Repositories;
using UserService.Infrastructure.Data;

namespace UserService.Infrastructure.Repositories;

public sealed class UserSessionRepository : IUserSessionRepository
{
    private readonly UserDbContext _db;

    public UserSessionRepository(UserDbContext db) => _db = db;

    public async Task AddAsync(UserSession session, CancellationToken cancellationToken = default)
        => await _db.UserSessions.AddAsync(session, cancellationToken);

    public Task<UserSession?> GetByIdAsync(Guid sessionId, CancellationToken cancellationToken = default)
        => _db.UserSessions.AsTracking().FirstOrDefaultAsync(x => x.Id == sessionId, cancellationToken);

    public Task<UserSession?> GetByRefreshTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default)
        => _db.UserSessions.AsTracking().FirstOrDefaultAsync(x => x.RefreshTokenHash == tokenHash, cancellationToken);

    public Task<UserSession?> GetByPreviousRefreshTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default)
        => _db.UserSessions.AsTracking().FirstOrDefaultAsync(x => x.PreviousRefreshTokenHash == tokenHash, cancellationToken);

    public async Task<IReadOnlyList<UserSession>> GetActiveForUserAsync(Guid userId, DateTime utcNow, CancellationToken cancellationToken = default)
        => await _db.UserSessions.AsTracking()
            .Where(x => x.UserId == userId && x.RevokedAt == null && x.ExpiresAt > utcNow)
            .OrderByDescending(x => x.LastSeenAt)
            .ToListAsync(cancellationToken);

    public Task<int> CountActiveAsync(DateTime utcNow, CancellationToken cancellationToken = default)
        => _db.UserSessions.CountAsync(x => x.RevokedAt == null && x.ExpiresAt > utcNow, cancellationToken);

    public Task<int> DeleteExpiredAsync(DateTime olderThan, CancellationToken cancellationToken = default)
        => _db.UserSessions
            .Where(x => x.ExpiresAt < olderThan || (x.RevokedAt != null && x.RevokedAt < olderThan))
            .ExecuteDeleteAsync(cancellationToken);
}
