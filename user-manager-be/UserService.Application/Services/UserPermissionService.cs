using UserService.Application.Interfaces.Services;
using UserService.Domain;
using UserService.Domain.Repositories;

namespace UserService.Application.Services;

public class UserPermissionService : IUserPermissionService
{
    private readonly IIdentityService _identityService;
    private readonly IPermissionRepository _permissionRepository;
    private readonly IUserApplicationRepository _userApplicationRepository;

    public UserPermissionService(
        IIdentityService identityService,
        IPermissionRepository permissionRepository,
        IUserApplicationRepository userApplicationRepository)
    {
        _identityService = identityService ?? throw new ArgumentNullException(nameof(identityService));
        _permissionRepository = permissionRepository ?? throw new ArgumentNullException(nameof(permissionRepository));
        _userApplicationRepository = userApplicationRepository ?? throw new ArgumentNullException(nameof(userApplicationRepository));
    }

    public async Task<IReadOnlyList<string>> GetPermissionCodesForUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var roleNames = await _identityService.GetUserRolesAsync(userId, cancellationToken);
        return await GetPermissionCodesByRoleNamesAsync(roleNames, cancellationToken);
    }

    public async Task<IReadOnlyList<string>> GetPermissionCodesForUserAsync(Guid userId, Guid applicationId, CancellationToken cancellationToken = default)
    {
        var membership = await _userApplicationRepository.GetByUserAndAppAsync(userId, applicationId, cancellationToken);
        if (membership == null || !membership.IsActive)
            return Array.Empty<string>();

        var roleNames = AssignedRolesFormat.Split(membership.Role);
        return await GetPermissionCodesByRoleNamesAsync(roleNames, cancellationToken);
    }

    private async Task<IReadOnlyList<string>> GetPermissionCodesByRoleNamesAsync(IEnumerable<string>? roleNames, CancellationToken cancellationToken)
    {
        var normalizedRoleNames = roleNames?
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Select(name => name.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (normalizedRoleNames == null || normalizedRoleNames.Length == 0)
            return Array.Empty<string>();

        var roleIds = new List<Guid>(normalizedRoleNames.Length);
        foreach (var name in normalizedRoleNames)
        {
            var roleId = await _identityService.GetRoleIdByNameAsync(name, cancellationToken);
            if (roleId.HasValue)
                roleIds.Add(roleId.Value);
        }

        if (roleIds.Count == 0)
            return Array.Empty<string>();

        return await _permissionRepository.GetPermissionCodesByRoleIdsAsync(roleIds, cancellationToken);
    }
}
