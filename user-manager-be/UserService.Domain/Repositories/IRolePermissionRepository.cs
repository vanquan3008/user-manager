using UserService.Domain.Entities;

namespace UserService.Domain.Repositories;

public interface IRolePermissionRepository
{
    Task<bool> ExistsAsync(Guid roleId, Guid permissionId, CancellationToken cancellationToken = default);
    Task AddAsync(RolePermission rolePermission, CancellationToken cancellationToken = default);
    Task RemoveAsync(Guid roleId, Guid permissionId, CancellationToken cancellationToken = default);
    Task RemoveAllForRoleAsync(Guid roleId, CancellationToken cancellationToken = default);
}
