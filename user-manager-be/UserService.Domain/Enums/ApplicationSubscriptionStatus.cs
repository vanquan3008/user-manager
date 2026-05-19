namespace UserService.Domain.Enums;

public enum ApplicationSubscriptionStatus
{
    Trialing = 0,
    Active = 1,
    PastDue = 2,
    Cancelled = 3,
    Expired = 4
}
