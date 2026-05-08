using UserService.Domain.Exceptions;
using UserService.Domain.ValueObjects;

namespace UserService.Domain.Entities;

public sealed class FamilySubscription : AuditableAggregateRoot<Guid>
{
    private FamilySubscription() : base() { }

    private FamilySubscription(
        Guid id,
        Guid familyPlanId,
        Guid subscriptionPlanId,
        DateTime startedAt)
        : base()
    {
        Id = id;
        FamilyPlanId = familyPlanId;
        SubscriptionPlanId = subscriptionPlanId;
        StartedAt = startedAt;
        MarkCreated();
    }
    
    public static FamilySubscription Create(
        Guid id,
        Guid familyPlanId,
        Guid subscriptionPlanId,
        DateTime startedAt)
    {
        return new FamilySubscription(id, familyPlanId, subscriptionPlanId, startedAt);
    }

    public Guid FamilyPlanId { get; private set; }
    public Guid SubscriptionPlanId { get; private set; }

    public FamilyPlan FamilyPlan { get; private set; } = null!;
    public SubscriptionPlan SubscriptionPlan { get; private set; } = null!;

    public DateTime StartedAt { get; private set; }
    public DateTime? ExpiresAt { get; private set; }
    public DateTime? RenewsAt { get; private set; }
    public string BillingCycle { get; private set; } = "monthly";

    public string? PaymentMethodId { get; private set; }
    
    // We can use Money if needed, but since it's just decimals in the original, we can keep it or use Money
    public Money? PricePerMonth { get; private set; }
    public Money? PricePerYear { get; private set; }

    public bool IsActive { get; private set; } = true;
    public bool IsExpired(DateTime utcNow) => ExpiresAt.HasValue && ExpiresAt <= utcNow && !IsActive;
    public bool AutoRenew { get; private set; } = true;
    public DateTime? CancelledAt { get; private set; }

    public StorageQuota StorageMBPerFamily { get; private set; } = StorageQuota.Create(0);

    public void UpgradeSubscriptionPlan(Guid newPlanId, DateTime? expiresAt, DateTime utcNow)
    {
        if (IsExpired(utcNow))
            throw new DomainOperationNotAllowedException("UpgradeSubscriptionPlan", "Cannot upgrade expired subscription");

        SubscriptionPlanId = newPlanId;
        ExpiresAt = expiresAt;
        Touch();
    }

    public void ExtendSubscription(DateTime newExpiresAt, DateTime utcNow)
    {
        if (newExpiresAt <= utcNow)
            throw new InvalidDomainObjectException("ExpiresAt", newExpiresAt, "Expiration must be in the future.");

        ExpiresAt = newExpiresAt;
        if (!CancelledAt.HasValue)
        {
            IsActive = true;
        }
        RenewsAt = newExpiresAt.AddSeconds(-1);
        Touch();
    }

    public void SetAutoRenewal(bool autoRenew)
    {
        AutoRenew = autoRenew;
        Touch();
    }

    public void CancelSubscription(DateTime cancelledAt)
    {
        if (CancelledAt.HasValue)
            throw new DomainOperationNotAllowedException("CancelSubscription", "Subscription is already cancelled");

        CancelledAt = cancelledAt;
        AutoRenew = false;
        IsActive = false;
        Touch();
    }

    public void UpdatePaymentMethod(string paymentMethodId)
    {
        PaymentMethodId = paymentMethodId;
        Touch();
    }
    
    public void UpdatePrices(Money pricePerMonth, Money pricePerYear)
    {
        PricePerMonth = pricePerMonth;
        PricePerYear = pricePerYear;
        Touch();
    }

    public void UpdateFeatureLimits(StorageQuota storageQuota)
    {
        StorageMBPerFamily = storageQuota;
        Touch();
    }
}
