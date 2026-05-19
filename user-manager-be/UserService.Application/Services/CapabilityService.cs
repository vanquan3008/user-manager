using UserService.Application.DTOs.Authorization;
using UserService.Application.Interfaces.Services;
using UserService.Domain.Constants;
using UserService.Domain.Exceptions;
using UserService.Domain.Interfaces;
using UserService.Domain.Repositories;

namespace UserService.Application.Services;

public sealed class CapabilityService : ICapabilityService
{
    private readonly IApplicationRepository _applicationRepository;
    private readonly IIdentityService _identityService;
    private readonly IApplicationAccessService _applicationAccessService;
    private readonly IUserPermissionService _userPermissionService;
    private readonly ISubscriptionRepository _subscriptionRepository;

    public CapabilityService(
        IApplicationRepository applicationRepository,
        IIdentityService identityService,
        IApplicationAccessService applicationAccessService,
        IUserPermissionService userPermissionService,
        ISubscriptionRepository subscriptionRepository)
    {
        _applicationRepository = applicationRepository ?? throw new ArgumentNullException(nameof(applicationRepository));
        _identityService = identityService ?? throw new ArgumentNullException(nameof(identityService));
        _applicationAccessService = applicationAccessService ?? throw new ArgumentNullException(nameof(applicationAccessService));
        _userPermissionService = userPermissionService ?? throw new ArgumentNullException(nameof(userPermissionService));
        _subscriptionRepository = subscriptionRepository ?? throw new ArgumentNullException(nameof(subscriptionRepository));
    }

    public async Task<ApplicationCapabilitiesDto> GetCapabilitiesAsync(
        Guid userId,
        Guid applicationId,
        CancellationToken cancellationToken = default)
    {
        var application = await _applicationRepository.GetByIdWithOwnerAsync(applicationId, cancellationToken)
            ?? throw new ApplicationNotFoundException(applicationId);

        application.EnsureActive();

        var roles = await _identityService.GetUserRolesAsync(userId, cancellationToken);
        if (RoleNames.ContainsSuperAdmin(roles))
        {
            var allPermissions = PermissionCodeMapper.GetCanonicalCodes()
                .OrderBy(code => code, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(code => code, _ => true, StringComparer.OrdinalIgnoreCase);
            var allFeatures = FeatureCodes.All
                .OrderBy(code => code, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(code => code, _ => true, StringComparer.OrdinalIgnoreCase);

            return new ApplicationCapabilitiesDto(applicationId, userId, allFeatures, allPermissions);
        }

        await _applicationAccessService.EnsureAccessAsync(userId, applicationId, roles, cancellationToken);

        var grantedPermissions = await _userPermissionService.GetPermissionCodesForUserAsync(
            userId,
            applicationId,
            cancellationToken);

        var normalizedPermissions = PermissionCodeMapper.NormalizeGrantedPermissions(grantedPermissions);
        var permissions = PermissionCodeMapper.GetCanonicalCodes()
            .OrderBy(code => code, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                code => code,
                code => normalizedPermissions.Contains(code),
                StringComparer.OrdinalIgnoreCase);

        var enabledFeatures = await GetEnabledFeaturesAsync(userId, applicationId, cancellationToken);
        var features = FeatureCodes.All
            .OrderBy(code => code, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                code => code,
                code => enabledFeatures.Contains(code),
                StringComparer.OrdinalIgnoreCase);

        return new ApplicationCapabilitiesDto(applicationId, userId, features, permissions);
    }

    private async Task<IReadOnlySet<string>> GetEnabledFeaturesAsync(
        Guid userId,
        Guid applicationId,
        CancellationToken cancellationToken)
    {
        var activeSubscription = await _subscriptionRepository.GetActiveSubscriptionAsync(
            userId,
            applicationId,
            cancellationToken);
        var enabledFeatureCodes = activeSubscription?.EntitlementSnapshot?.Features ?? Array.Empty<string>();

        return enabledFeatureCodes
            .Select(code => code.Trim().ToUpperInvariant())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }
}
