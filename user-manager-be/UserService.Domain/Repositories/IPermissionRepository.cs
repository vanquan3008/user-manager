using UserService.Domain.Entities;

namespace UserService.Domain.Repositories;

public interface IPermissionRepository : IRepository<Permission>
{
    Task<IReadOnlyList<string>> GetPermissionCodesByRoleIdsAsync(IEnumerable<Guid> roleIds, CancellationToken cancellationToken = default);
    Task<IReadOnlyDictionary<Guid, string>> GetPermissionCodesCsvByRoleIdsAsync(
        IEnumerable<Guid> roleIds,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Permission>> GetPermissionsByRoleIdAsync(Guid roleId, CancellationToken cancellationToken = default);
    Task<Permission?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Permission>> GetByCodeAsync(IEnumerable<string> codes, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Permission>> GetByScopeAsync(IEnumerable<string> scopes, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Permission>> GetByCategoryAsync(string category, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Permission>> GetActivePermissionsAsync(CancellationToken cancellationToken = default);
    Task<bool> ExistsByCodeAsync(string code, CancellationToken cancellationToken = default);
}
