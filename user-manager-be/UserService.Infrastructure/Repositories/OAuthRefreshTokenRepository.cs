using Microsoft.EntityFrameworkCore;
using UserService.Domain.Entities;
using UserService.Domain.Repositories;
using UserService.Infrastructure.Data;
using UserService.Domain.Interfaces;

namespace UserService.Infrastructure.Repositories;

public sealed class OAuthRefreshTokenRepository : Repository<OAuthRefreshToken>, IOAuthRefreshTokenRepository
{
    private readonly IClock _clock;
    
    public OAuthRefreshTokenRepository(UserDbContext context, IClock clock) : base(context) 
    { 
        _clock = clock;
    }

    public Task<OAuthRefreshToken?> GetByHashAsync(string tokenHash, CancellationToken cancellationToken = default)
    {
        var normalized = (tokenHash ?? string.Empty).Trim();
        return _dbSet.AsTracking().FirstOrDefaultAsync(x => x.TokenHash == normalized, cancellationToken);
    }

    public async Task<int> RevokeAllForUserClientAsync(Guid userId, Guid applicationId, string reason, CancellationToken cancellationToken = default)
    {
        var tokens = await _dbSet.AsTracking()
            .Where(x => x.UserId == userId && x.ApplicationId == applicationId && x.RevokedAtUtc == null)
            .ToListAsync(cancellationToken);

        foreach (var t in tokens)
            t.Revoke(_clock.UtcNow, reason);
        return tokens.Count;
    }

    public async Task<IReadOnlyList<Guid>> RevokeAllForSessionAsync(Guid sessionId, string reason, CancellationToken cancellationToken = default)
    {
        var tokens = await _dbSet.AsTracking()
            .Where(x => x.SessionId == sessionId && x.RevokedAtUtc == null)
            .ToListAsync(cancellationToken);
        foreach (var t in tokens)
            t.Revoke(_clock.UtcNow, reason);
        return tokens.Select(t => t.ApplicationId).Distinct().ToList();
    }

    public async Task<IReadOnlyList<Guid>> RevokeAllForUserAsync(Guid userId, string reason, CancellationToken cancellationToken = default)
    {
        var tokens = await _dbSet.AsTracking()
            .Where(x => x.UserId == userId && x.RevokedAtUtc == null)
            .ToListAsync(cancellationToken);
        foreach (var t in tokens)
            t.Revoke(_clock.UtcNow, reason);
        return tokens.Select(t => t.ApplicationId).Distinct().ToList();
    }

    public Task<int> DeleteExpiredAsync(DateTime olderThan, CancellationToken cancellationToken = default)
        => _dbSet.IgnoreQueryFilters().Where(x => x.ExpiresAtUtc < olderThan).ExecuteDeleteAsync(cancellationToken);
}
