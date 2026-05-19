using UserService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace UserService.Domain.Repositories;

public interface ISubscriptionRepository : IRepository<Subscription>
{
    /// <summary>
    /// Get active subscription for a user within a specific application (no tracking, for reads).
    /// </summary>
    Task<Subscription?> GetActiveSubscriptionAsync(Guid userId, Guid applicationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get active subscription with tracking for update operations (e.g. upgrade, extend, cancel).
    /// </summary>
    Task<Subscription?> GetActiveSubscriptionForUpdateAsync(Guid userId, Guid applicationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all subscriptions for user within a specific application (including history)
    /// </summary>
    Task<IEnumerable<Subscription>> GetUserSubscriptionsAsync(Guid userId, Guid applicationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get latest subscription for user within a specific application
    /// </summary>
    Task<Subscription?> GetLatestSubscriptionAsync(Guid userId, Guid applicationId, CancellationToken cancellationToken = default);
}
