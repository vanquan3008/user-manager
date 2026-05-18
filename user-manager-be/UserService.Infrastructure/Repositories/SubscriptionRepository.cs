using Microsoft.EntityFrameworkCore;
using UserService.Domain.Entities;
using UserService.Domain.Repositories;
using UserService.Infrastructure.Data;

namespace UserService.Infrastructure.Repositories;

/// <summary>
/// Subscription repository - implements subscription-specific queries.
/// </summary>
public sealed class SubscriptionRepository : Repository<Subscription>, ISubscriptionRepository
{
    public SubscriptionRepository(UserDbContext context) : base(context)
    {
    }

    public async Task<Subscription?> GetActiveSubscriptionAsync(Guid userId, Guid applicationId, CancellationToken cancellationToken = default)
    {
        var utcNow = DateTime.UtcNow;
        return await _dbSet
            .AsNoTracking()
            .Where(s =>
                s.UserId == userId &&
                s.ApplicationId == applicationId &&
                s.IsActive &&
                s.CancelledAt == null &&
                (!s.ExpiresAt.HasValue || s.ExpiresAt > utcNow))
            .OrderByDescending(s => s.StartedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Subscription?> GetActiveSubscriptionForUpdateAsync(Guid userId, Guid applicationId, CancellationToken cancellationToken = default)
    {
        var utcNow = DateTime.UtcNow;
        return await _dbSet
            .Where(s =>
                s.UserId == userId &&
                s.ApplicationId == applicationId &&
                s.IsActive &&
                s.CancelledAt == null &&
                (!s.ExpiresAt.HasValue || s.ExpiresAt > utcNow))
            .OrderByDescending(s => s.StartedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IEnumerable<Subscription>> GetUserSubscriptionsAsync(Guid userId, Guid applicationId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(s => s.UserId == userId && s.ApplicationId == applicationId)
            .OrderByDescending(s => s.StartedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Subscription?> GetLatestSubscriptionAsync(Guid userId, Guid applicationId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(s => s.UserId == userId && s.ApplicationId == applicationId)
            .OrderByDescending(s => s.StartedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }
}


