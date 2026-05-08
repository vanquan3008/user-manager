namespace UserService.Domain.Entities;

/// <summary>
/// Gói subscription gắn nhiều role (AspNetRoles) đã được liên kết với application qua bảng ApplicationRoles.
/// </summary>
public sealed class SubscriptionPlanRoleAssignment
{
    public Guid SubscriptionPlanId { get; private set; }
    public Guid RoleId { get; private set; }
    public SubscriptionPlan SubscriptionPlan { get; private set; } = null!;

    private SubscriptionPlanRoleAssignment()
    {
    }

    public SubscriptionPlanRoleAssignment(Guid subscriptionPlanId, Guid roleId)
    {
        SubscriptionPlanId = subscriptionPlanId;
        RoleId = roleId;
    }
}
