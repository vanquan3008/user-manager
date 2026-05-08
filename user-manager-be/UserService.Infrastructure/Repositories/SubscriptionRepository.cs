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
        return await _dbSet
            .Include(s => s.Members)
                .ThenInclude(m => m.User)
            .Include(s => s.Members)
                .ThenInclude(m => m.RoleType)
            .AsNoTracking()
            .Where(s => s.UserId == userId && s.ApplicationId == applicationId && s.IsActive)
            .OrderByDescending(s => s.StartedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Subscription?> GetByIdWithMembersAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(s => s.Members)
            .Where(s => s.Id == id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Subscription?> GetActiveSubscriptionForUpdateAsync(Guid userId, Guid applicationId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(s => s.Members)
            .Where(s => s.UserId == userId && s.ApplicationId == applicationId && s.IsActive)
            .OrderByDescending(s => s.StartedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IEnumerable<Subscription>> GetUserSubscriptionsAsync(Guid userId, Guid applicationId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(s => s.Members)
                .ThenInclude(m => m.User)
            .Include(s => s.Members)
                .ThenInclude(m => m.RoleType)
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


