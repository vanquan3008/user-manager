namespace UserService.Domain.Constants;

public static partial class PermissionCodes
{
    // Subscription catalog and plan operations
    public const string ViewSubscriptions = "SUBSCRIPTION.READ";
    public const string ManageSubscriptions = "SUBSCRIPTION.MANAGE";

    // Subscription operations
    public const string ActivateSubscription = "SUBSCRIPTION.ACTIVATE";
    public const string UpgradeSubscription = "SUBSCRIPTION.UPGRADE";
    public const string ExtendSubscription = "SUBSCRIPTION.EXTEND";
    public const string CancelSubscription = "SUBSCRIPTION.CANCEL";
}
