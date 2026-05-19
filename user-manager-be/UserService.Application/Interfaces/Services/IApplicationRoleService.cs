using UserService.Application.Common;

namespace UserService.Application.Interfaces.Services;

/// <summary>
/// Service for managing application-scoped roles (abstracts RoleManager).
/// </summary>
public interface IApplicationRoleService
{
    Task<IReadOnlyList<ApplicationRoleDto>> GetRolesForApplicationAsync(Guid applicationId, CancellationToken cancellationToken = default);

    Task<bool> RoleExistsForApplicationAsync(Guid applicationId, string roleName, CancellationToken cancellationToken = default);

    Task<ApplicationRoleResult> CreateRoleAsync(Guid applicationId, string name, string? description, CancellationToken cancellationToken = default);

    Task<ApplicationRoleResult> UpdateRoleAsync(Guid applicationId, string roleName, string? description, CancellationToken cancellationToken = default);

    Task<ApplicationRoleResult> DeleteRoleAsync(Guid applicationId, string roleName, CancellationToken cancellationToken = default);

    Task<bool> RoleExistsForUserAssignmentAsync(Guid applicationId, string roleName, CancellationToken cancellationToken = default);

    Task<Guid?> GetRoleIdForUserAssignmentAsync(Guid applicationId, string roleName, CancellationToken cancellationToken = default);
}
