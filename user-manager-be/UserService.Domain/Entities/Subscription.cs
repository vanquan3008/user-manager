using UserService.Domain.Enums;
using UserService.Domain.Events;
using UserService.Domain.Exceptions;
using UserService.Domain.ValueObjects;

namespace UserService.Domain.Entities;

public sealed class Subscription : AuditableAggregateRoot<Guid>
{
    private Subscription() : base() { }

    private Subscription(
        Guid id,
        Guid userId,
        Guid applicationId,
        Guid? subscriptionPlanId,
        SubscriptionType type,
        DateTime startedAt,
        int maxMembers = 1,
        EntitlementSnapshot? entitlementSnapshot = null)
        : base()
    {
        Id = id;
        UserId = userId;
        ApplicationId = applicationId;
        SubscriptionPlanId = subscriptionPlanId;
        SubscriptionType = type;
        MaxMembers = maxMembers;
        _entitlementSnapshot = entitlementSnapshot;
        StartedAt = startedAt;
        
        MarkCreated();

        if (type != SubscriptionType.Free)
            AddDomainEvent(new SubscriptionActivatedEvent(id, userId, type));
    }

    public static Subscription AssignPlan(
        Guid id,
        Guid userId,
        Guid applicationId,
        SubscriptionPlan plan,
        IReadOnlyCollection<string> featureCodes,
        SubscriptionType type,
        DateTime startedAt,
        int maxMembers = 1)
    {
        if (plan == null)
            throw new ArgumentNullException(nameof(plan));
        if (!plan.ApplicationId.HasValue || plan.ApplicationId.Value != applicationId)
            throw new InvalidDomainObjectException("ApplicationId", applicationId, "Plan must belong to the same application.");

        var snapshot = plan.CreateEntitlementSnapshot(featureCodes, maxMembers);
        return new Subscription(id, userId, applicationId, plan.Id, type, startedAt, maxMembers, snapshot);
    }

    public Guid UserId { get; private set; }
    public Guid ApplicationId { get; private set; }
    public Guid? SubscriptionPlanId { get; private set; }
    
    private EntitlementSnapshot? _entitlementSnapshot;
    public EntitlementSnapshot? EntitlementSnapshot => _entitlementSnapshot;

    public User User { get; private set; } = null!;
    public AppApplication Application { get; private set; } = null!;
    public SubscriptionPlan? SubscriptionPlan { get; private set; }

    public SubscriptionType SubscriptionType { get; private set; }
    public DateTime StartedAt { get; private set; }

    public int MaxMembers { get; private set; }
    public int CurrentMemberCount => 1;

    public DateTime? ExpiresAt { get; private set; }
    public DateTime? CancelledAt { get; private set; }
    
    public bool IsActive { get; private set; } = true;
    public bool IsExpired(DateTime utcNow) => ExpiresAt.HasValue && ExpiresAt <= utcNow;

    public void UpgradeSubscription(
        SubscriptionType newType,
        Guid? newSubscriptionPlanId,
        DateTime? expiresAt,
        EntitlementSnapshot newSnapshot,
        DateTime utcNow)
    {
        if (newType == SubscriptionType.Free)
            throw new DomainOperationNotAllowedException("UpgradeSubscription", "Cannot upgrade to Free subscription");
        if (IsExpired(utcNow))
            throw new DomainOperationNotAllowedException("UpgradeSubscription", "Cannot upgrade expired subscription");
        if (newSnapshot == null)
            throw new ArgumentNullException(nameof(newSnapshot));

        SubscriptionType = newType;
        if (newSubscriptionPlanId.HasValue) SubscriptionPlanId = newSubscriptionPlanId;
        _entitlementSnapshot = newSnapshot;
        ExpiresAt = expiresAt;
        if (!CancelledAt.HasValue)
            IsActive = true;
        Touch();

        AddDomainEvent(new SubscriptionUpgradedEvent(Id, UserId, newType, expiresAt));
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
        Touch();

        AddDomainEvent(new SubscriptionExtendedEvent(Id, UserId, newExpiresAt));
    }

    public void CancelSubscription(DateTime cancelledAt)
    {
        if (CancelledAt.HasValue)
            throw new DomainOperationNotAllowedException("CancelSubscription", "Subscription is already cancelled");

        CancelledAt = cancelledAt;
        IsActive = false;
        Touch();

        AddDomainEvent(new SubscriptionCancelledEvent(Id, UserId));
    }

    public bool HasFeature(string featureCode)
    {
        return _entitlementSnapshot?.HasFeature(featureCode) ?? false;
    }

    public int? GetQuota(string quotaKey)
    {
        return _entitlementSnapshot?.GetQuota(quotaKey);
    }
}
