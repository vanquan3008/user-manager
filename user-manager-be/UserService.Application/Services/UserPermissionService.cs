using UserService.Application.Interfaces.Services;
using UserService.Domain;
using UserService.Domain.Constants;
using UserService.Domain.Repositories;

namespace UserService.Application.Services;

public class UserPermissionService : IUserPermissionService
{
    private readonly IIdentityService _identityService;
    private readonly IPermissionRepository _permissionRepository;
    private readonly IApplicationMembershipRepository _userApplicationRepository;
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly ISubscriptionPlanRepository _subscriptionPlanRepository;

    public UserPermissionService(
        IIdentityService identityService,
        IPermissionRepository permissionRepository,
        IApplicationMembershipRepository userApplicationRepository,
        ISubscriptionRepository subscriptionRepository,
        ISubscriptionPlanRepository subscriptionPlanRepository)
    {
        _identityService = identityService ?? throw new ArgumentNullException(nameof(identityService));
        _permissionRepository = permissionRepository ?? throw new ArgumentNullException(nameof(permissionRepository));
        _userApplicationRepository = userApplicationRepository ?? throw new ArgumentNullException(nameof(userApplicationRepository));
        _subscriptionRepository = subscriptionRepository ?? throw new ArgumentNullException(nameof(subscriptionRepository));
        _subscriptionPlanRepository = subscriptionPlanRepository ?? throw new ArgumentNullException(nameof(subscriptionPlanRepository));
    }

    public async Task<IReadOnlyList<string>> GetPermissionCodesForUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var roleNames = await _identityService.GetUserRolesAsync(userId, cancellationToken);
        return await GetPermissionCodesByRoleNamesAsync(roleNames, cancellationToken);
    }

    public async Task<IReadOnlyList<string>> GetPermissionCodesForUserAsync(Guid userId, Guid applicationId, CancellationToken cancellationToken = default)
    {
        var roleNames = await _identityService.GetUserRolesAsync(userId, cancellationToken);
        if (RoleNames.ContainsSuperAdmin(roleNames))
            return await GetPermissionCodesByRoleNamesAsync(roleNames, cancellationToken);

        var membership = await _userApplicationRepository.GetByUserAndAppAsync(userId, applicationId, cancellationToken);
        if (membership == null || !membership.IsActive)
            return Array.Empty<string>();

        var subscription = await _subscriptionRepository.GetActiveSubscriptionAsync(userId, applicationId, cancellationToken);
        if (subscription?.SubscriptionPlanId is not Guid subscriptionPlanId)
            return Array.Empty<string>();

        var rolePermissionCodes = await GetPermissionCodesByRoleIdsAsync(new[] { membership.RoleId }, cancellationToken);
        var packagePermissionCodes = await _subscriptionPlanRepository.GetPermissionCodesByPlanIdAsync(subscriptionPlanId, cancellationToken);

        return rolePermissionCodes
            .Intersect(packagePermissionCodes, StringComparer.OrdinalIgnoreCase)
            .OrderBy(code => code, StringComparer.OrdinalIgnoreCase)
            .ToArray();
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

        return await GetPermissionCodesByRoleIdsAsync(roleIds, cancellationToken);
    }

    private async Task<IReadOnlyList<string>> GetPermissionCodesByRoleIdsAsync(IEnumerable<Guid> roleIds, CancellationToken cancellationToken)
    {
        var permissionCodes = await _permissionRepository.GetPermissionCodesByRoleIdsAsync(roleIds, cancellationToken);
        return PermissionCodeMapper.NormalizeGrantedPermissions(permissionCodes)
            .OrderBy(code => code, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }
}
