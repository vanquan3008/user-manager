using UserService.Domain.Enums;
using UserService.Domain.Exceptions;
using UserService.Domain.ValueObjects;

namespace UserService.Domain.Entities;

/// <summary>
/// Canonical subscription for one SaaS application. At most one active row should exist per application.
/// </summary>
public sealed class ApplicationSubscription : AuditableAggregateRoot<Guid>
{
    private ApplicationSubscription() : base() { }

    private ApplicationSubscription(
        Guid id,
        Guid applicationId,
        Guid subscriptionPlanId,
        ApplicationSubscriptionStatus status,
        DateTime startedAt,
        DateTime? endsAt,
        EntitlementSnapshot? entitlementSnapshot)
        : base()
    {
        if (applicationId == Guid.Empty)
            throw new InvalidDomainObjectException(nameof(applicationId), applicationId, "ApplicationId is required.");
        if (subscriptionPlanId == Guid.Empty)
            throw new InvalidDomainObjectException(nameof(subscriptionPlanId), subscriptionPlanId, "SubscriptionPlanId is required.");
        if (endsAt.HasValue && endsAt.Value <= startedAt)
            throw new InvalidDomainObjectException(nameof(endsAt), endsAt.Value, "EndsAt must be after StartedAt.");

        Id = id == Guid.Empty ? Guid.NewGuid() : id;
        ApplicationId = applicationId;
        SubscriptionPlanId = subscriptionPlanId;
        Status = status;
        StartedAt = startedAt;
        EndsAt = endsAt;
        EntitlementSnapshot = entitlementSnapshot;
        MarkCreated();
    }

    public static ApplicationSubscription Start(
        Guid id,
        Guid applicationId,
        Guid subscriptionPlanId,
        DateTime startedAt,
        DateTime? endsAt,
        EntitlementSnapshot? entitlementSnapshot)
        => new(id, applicationId, subscriptionPlanId, ApplicationSubscriptionStatus.Active, startedAt, endsAt, entitlementSnapshot);

    public Guid ApplicationId { get; private set; }
    public Guid SubscriptionPlanId { get; private set; }
    public ApplicationSubscriptionStatus Status { get; private set; }
    public DateTime StartedAt { get; private set; }
    public DateTime? EndsAt { get; private set; }
    public DateTime? CancelledAt { get; private set; }
    public EntitlementSnapshot? EntitlementSnapshot { get; private set; }

    public AppApplication Application { get; private set; } = null!;
    public SubscriptionPlan SubscriptionPlan { get; private set; } = null!;

    public bool IsCurrentlyActive(DateTime utcNow)
        => Status is ApplicationSubscriptionStatus.Trialing or ApplicationSubscriptionStatus.Active or ApplicationSubscriptionStatus.PastDue
           && (!EndsAt.HasValue || EndsAt > utcNow)
           && !CancelledAt.HasValue;

    public void ChangePlan(Guid subscriptionPlanId, EntitlementSnapshot? entitlementSnapshot, DateTime? endsAt)
    {
        if (subscriptionPlanId == Guid.Empty)
            throw new InvalidDomainObjectException(nameof(subscriptionPlanId), subscriptionPlanId, "SubscriptionPlanId is required.");
        if (endsAt.HasValue && endsAt.Value <= StartedAt)
            throw new InvalidDomainObjectException(nameof(endsAt), endsAt.Value, "EndsAt must be after StartedAt.");

        SubscriptionPlanId = subscriptionPlanId;
        EntitlementSnapshot = entitlementSnapshot;
        EndsAt = endsAt;
        Touch();
    }

    public void MarkPastDue()
    {
        Status = ApplicationSubscriptionStatus.PastDue;
        Touch();
    }

    public void Cancel(DateTime cancelledAt)
    {
        Status = ApplicationSubscriptionStatus.Cancelled;
        CancelledAt = cancelledAt;
        Touch();
    }

    public void Expire()
    {
        Status = ApplicationSubscriptionStatus.Expired;
        Touch();
    }
}
