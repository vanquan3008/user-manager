using UserService.Application.Interfaces.Services;
using UserService.Application.Common;
using UserService.Domain.Constants;
using UserService.Domain.Repositories;

namespace UserService.Application.Services;

public sealed class ApplicationAccessService : IApplicationAccessService
{
    private readonly IIdentityService _identityService;
    private readonly IApplicationMembershipRepository _memberships;
    private readonly ISubscriptionRepository _subscriptions;

    public ApplicationAccessService(
        IIdentityService identityService,
        IApplicationMembershipRepository memberships,
        ISubscriptionRepository subscriptions)
    {
        _identityService = identityService;
        _memberships = memberships;
        _subscriptions = subscriptions;
    }

    public async Task<bool> HasAccessAsync(
        Guid userId,
        Guid applicationId,
        IEnumerable<string>? roleNames = null,
        CancellationToken cancellationToken = default)
    {
        var evaluation = await EvaluateAsync(userId, applicationId, roleNames, cancellationToken);
        return evaluation.HasAccess;
    }

    public async Task<ApplicationAccessEvaluation> EvaluateAsync(
        Guid userId,
        Guid applicationId,
        IEnumerable<string>? roleNames = null,
        CancellationToken cancellationToken = default)
    {
        var resolvedRoles = roleNames?.ToArray()
            ?? await _identityService.GetUserRolesAsync(userId, cancellationToken);

        if (RoleNames.ContainsSuperAdmin(resolvedRoles))
            return new ApplicationAccessEvaluation(true, false, false);

        var membership = await _memberships.GetByUserAndAppAsync(userId, applicationId, cancellationToken);
        if (membership == null || !membership.IsActive)
            return new ApplicationAccessEvaluation(false, false, false);

        var subscription = await _subscriptions.GetActiveSubscriptionAsync(userId, applicationId, cancellationToken);
        return new ApplicationAccessEvaluation(
            HasSystemRoleAccess: false,
            HasActiveMembership: true,
            HasActiveSubscription: subscription != null);
    }

    public async Task EnsureAccessAsync(
        Guid userId,
        Guid applicationId,
        IEnumerable<string>? roleNames = null,
        CancellationToken cancellationToken = default)
    {
        var hasAccess = await HasAccessAsync(userId, applicationId, roleNames, cancellationToken);
        if (!hasAccess)
            throw new UnauthorizedAccessException("User does not have active access to this application.");
    }
}
