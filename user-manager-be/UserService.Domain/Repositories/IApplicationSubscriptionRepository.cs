using UserService.Domain.Entities;

namespace UserService.Domain.Repositories;

public interface IApplicationSubscriptionRepository : IRepository<ApplicationSubscription>
{
    Task<ApplicationSubscription?> GetActiveByApplicationAsync(
        Guid applicationId,
        DateTime utcNow,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<string>> GetEnabledFeatureCodesAsync(
        Guid applicationId,
        DateTime utcNow,
        CancellationToken cancellationToken = default);
}
