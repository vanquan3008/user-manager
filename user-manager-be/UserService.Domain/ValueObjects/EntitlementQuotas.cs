namespace UserService.Domain.ValueObjects;

public sealed record EntitlementQuotas(
    int MaxUsers,
    int MaxApplications,
    int MaxStorageMb,
    int MaxMembers,
    int? MaxApiCalls = null)
{
    public const int Unlimited = -1;

    public bool IsUnlimitedUsers => MaxUsers == Unlimited;
    public bool IsUnlimitedApplications => MaxApplications == Unlimited;
    public bool IsUnlimitedStorage => MaxStorageMb == Unlimited;
    public bool IsUnlimitedApiCalls => MaxApiCalls is null or Unlimited;
}
