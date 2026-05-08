using UserService.Domain.Entities;

namespace UserService.Domain.Repositories;

public interface ISubscriptionPlanRepository : IRepository<SubscriptionPlan>
{
    Task<IEnumerable<SubscriptionPlan>> GetByApplicationIdAsync(Guid applicationId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SubscriptionPlan>> GetCatalogPlansAsync(Guid? applicationId, CancellationToken cancellationToken = default);
    Task<SubscriptionPlan?> GetByIdWithRoleAssignmentsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyDictionary<Guid, string>> GetRoleNamesByIdsAsync(IEnumerable<Guid> roleIds, CancellationToken cancellationToken = default);
}
