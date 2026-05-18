namespace UserService.Domain.Entities;

/// <summary>
/// Permission bundle exposed by a package/subscription plan inside an application client.
/// </summary>
public sealed class SubscriptionPlanPermission
{
    private SubscriptionPlanPermission()
    {
    }

    public SubscriptionPlanPermission(Guid subscriptionPlanId, Guid permissionId)
    {
        if (subscriptionPlanId == Guid.Empty)
            throw new ArgumentException("SubscriptionPlanId is required.", nameof(subscriptionPlanId));
        if (permissionId == Guid.Empty)
            throw new ArgumentException("PermissionId is required.", nameof(permissionId));

        SubscriptionPlanId = subscriptionPlanId;
        PermissionId = permissionId;
    }

    public Guid SubscriptionPlanId { get; private set; }
    public Guid PermissionId { get; private set; }

    public SubscriptionPlan SubscriptionPlan { get; private set; } = null!;
    public Permission Permission { get; private set; } = null!;
}
