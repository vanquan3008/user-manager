namespace UserService.Domain.Constants;

public static partial class PermissionCodes
{
    // Subscription catalog (plans / license packages)
    public const string ViewSubscriptions = "subscriptions.view";
    public const string ManageSubscriptions = "subscriptions.manage";

    // Subscription operations (each API separate)
    public const string ActivateSubscription = "subscriptions.activate";
    public const string ActivateSubscriptionPackage = "subscriptions.activate-package";
    public const string UpgradeSubscription = "subscriptions.upgrade";
    public const string UpgradeSubscriptionPackage = "subscriptions.upgrade-package";
    public const string ExtendSubscription = "subscriptions.extend";
    public const string CancelSubscription = "subscriptions.cancel";
}

