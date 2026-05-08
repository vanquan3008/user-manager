using UserService.Application.Common;

namespace UserService.Application.Interfaces.Services;

/// <summary>
/// Service for managing global roles in AspNetRoles (application scope uses ApplicationRoles).
/// Only Admin and SuperAdmin should use these operations.
/// </summary>
public interface ISystemRoleService
{
    Task<IReadOnlyList<RoleDto>> GetGlobalRolesAsync(CancellationToken cancellationToken = default);

    Task<RoleDto?> GetGlobalRoleByIdAsync(Guid roleId, CancellationToken cancellationToken = default);
    Task<RoleBasicDto?> GetRoleByIdAsync(Guid roleId, CancellationToken cancellationToken = default);

    Task<RoleDto?> GetGlobalRoleByNameAsync(string roleName, CancellationToken cancellationToken = default);

    Task<ApplicationRoleResult> CreateGlobalRoleAsync(string name, string? description, string? permissions = null, CancellationToken cancellationToken = default);

    Task<ApplicationRoleResult> UpdateGlobalRoleAsync(Guid roleId, string? description, string? permissions = null, CancellationToken cancellationToken = default);

    Task<ApplicationRoleResult> DeleteGlobalRoleAsync(Guid roleId, CancellationToken cancellationToken = default);

    Task<bool> GlobalRoleExistsAsync(string roleName, CancellationToken cancellationToken = default);
}
