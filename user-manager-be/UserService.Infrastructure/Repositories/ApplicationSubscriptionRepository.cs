using Microsoft.EntityFrameworkCore;
using UserService.Domain.Entities;
using UserService.Domain.Enums;
using UserService.Domain.Repositories;
using UserService.Infrastructure.Data;

namespace UserService.Infrastructure.Repositories;

public sealed class ApplicationSubscriptionRepository
    : Repository<ApplicationSubscription>, IApplicationSubscriptionRepository
{
    private static readonly ApplicationSubscriptionStatus[] ActiveStatuses =
    [
        ApplicationSubscriptionStatus.Trialing,
        ApplicationSubscriptionStatus.Active,
        ApplicationSubscriptionStatus.PastDue
    ];

    public ApplicationSubscriptionRepository(UserDbContext context) : base(context)
    {
    }

    public Task<ApplicationSubscription?> GetActiveByApplicationAsync(
        Guid applicationId,
        DateTime utcNow,
        CancellationToken cancellationToken = default)
    {
        var context = (UserDbContext)_context;

        return context.ApplicationSubscriptions
            .AsNoTracking()
            .Where(subscription =>
                subscription.ApplicationId == applicationId &&
                ActiveStatuses.Contains(subscription.Status) &&
                subscription.CancelledAt == null &&
                (!subscription.EndsAt.HasValue || subscription.EndsAt > utcNow))
            .OrderByDescending(subscription => subscription.StartedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<string>> GetEnabledFeatureCodesAsync(
        Guid applicationId,
        DateTime utcNow,
        CancellationToken cancellationToken = default)
    {
        var context = (UserDbContext)_context;

        var activePlanId = await context.ApplicationSubscriptions
            .AsNoTracking()
            .Where(subscription =>
                subscription.ApplicationId == applicationId &&
                ActiveStatuses.Contains(subscription.Status) &&
                subscription.CancelledAt == null &&
                (!subscription.EndsAt.HasValue || subscription.EndsAt > utcNow))
            .OrderByDescending(subscription => subscription.StartedAt)
            .Select(subscription => (Guid?)subscription.SubscriptionPlanId)
            .FirstOrDefaultAsync(cancellationToken);

        if (!activePlanId.HasValue)
            return Array.Empty<string>();

        return await context.PlanFeatures
            .AsNoTracking()
            .Include(planFeature => planFeature.Feature)
            .Where(planFeature =>
                planFeature.SubscriptionPlanId == activePlanId.Value &&
                planFeature.IsEnabled &&
                planFeature.Feature.IsActive)
            .Select(planFeature => planFeature.Feature.Code)
            .Distinct()
            .OrderBy(code => code)
            .ToListAsync(cancellationToken);
    }
}
