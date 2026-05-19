using UserService.Domain.Entities;

namespace UserService.Domain.Repositories;

public interface ISubscriptionPlanRepository : IRepository<SubscriptionPlan>
{
    Task<IEnumerable<SubscriptionPlan>> GetByApplicationIdAsync(Guid applicationId, CancellationToken cancellationToken = default);
    Task<SubscriptionPlan?> GetByIdWithCatalogAsync(Guid planId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SubscriptionPlan>> GetCatalogPlansAsync(Guid? applicationId, CancellationToken cancellationToken = default);
    Task<IReadOnlyDictionary<Guid, IReadOnlyList<string>>> GetFeatureCodesByPlanIdsAsync(IEnumerable<Guid> planIds, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> GetFeatureCodesByPlanIdAsync(Guid planId, CancellationToken cancellationToken = default);
    Task<IReadOnlyDictionary<Guid, IReadOnlyList<string>>> GetPermissionCodesByPlanIdsAsync(IEnumerable<Guid> planIds, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> GetPermissionCodesByPlanIdAsync(Guid planId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Guid>> ResolveFeatureIdsAsync(IEnumerable<string> featureCodes, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Guid>> ResolvePermissionIdsAsync(IEnumerable<string> permissionCodes, CancellationToken cancellationToken = default);
}
