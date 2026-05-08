using UserService.Domain.Entities;
using UserService.Domain.ValueObjects;

namespace UserService.Domain.Repositories;


public interface IApplicationRoleRepository
{
    Task<IReadOnlyList<(ApplicationRole Mapping, RoleLookup Role)>> GetMappingsWithRolesForApplicationAsync(
        Guid applicationId,
        CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid applicationId, Guid roleId, CancellationToken cancellationToken = default);
    Task AddAsync(ApplicationRole entity, CancellationToken cancellationToken = default);
    Task<ApplicationRole?> GetTrackedMappingAsync(Guid applicationId, Guid roleId, CancellationToken cancellationToken = default);
    void Remove(ApplicationRole entity);
    Task<int> CountRolesLinkedToApplicationAsync(Guid applicationId, IEnumerable<Guid> roleIds, CancellationToken cancellationToken = default);
}
