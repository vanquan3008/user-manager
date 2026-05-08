using UserService.Domain.Constants;
using UserService.Domain.Exceptions;

namespace UserService.Domain.Entities;

public sealed class SubscriptionMember : Entity<Guid>
{
    private SubscriptionMember() : base() { }

    private SubscriptionMember(
        Guid id,
        Guid subscriptionId,
        Guid userId,
        Guid roleTypeId,
        DateTime joinedAt)
        : base()
    {
        Id = id;
        SubscriptionId = subscriptionId;
        UserId = userId;
        RoleTypeId = roleTypeId;
        IsActive = true;
        JoinedAt = joinedAt;
        MarkCreated();
    }
    
    public static SubscriptionMember Create(
        Guid id,
        Guid subscriptionId,
        Guid userId,
        Guid roleTypeId,
        DateTime joinedAt)
    {
        return new SubscriptionMember(id, subscriptionId, userId, roleTypeId, joinedAt);
    }

    public Guid SubscriptionId { get; private set; }
    public Guid UserId { get; private set; }
    public Guid RoleTypeId { get; private set; }

    public Subscription Subscription { get; private set; } = null!;
    public User User { get; private set; } = null!;
    public SubscriptionRoleType RoleType { get; private set; } = null!;

    public bool IsAdmin
    {
        get => RoleType?.IsAdmin ?? SubscriptionRoleTypeIds.IsAdmin(RoleType?.Name);
        set { /* EF materialization */ }
    }

    public bool IsOwner
    {
        get => SubscriptionRoleTypeIds.IsOwner(RoleType?.Name)
               || RoleTypeId == SubscriptionRoleTypeIds.Owner;
        set { /* EF materialization */ }
    }
    
    public bool IsActive { get; private set; }

    public bool CanManageMembers => IsAdmin;
    public bool CanChangePaymentMethod => IsAdmin;
    public bool CanAccessAllApps => true;

    public decimal ContributionPercentage { get; private set; } = 0m;
    public decimal TotalPaid { get; private set; }

    public bool IsPrimaryPayer { get; private set; }
    public DateTime JoinedAt { get; private set; }
    public DateTime? LastActivityAt { get; private set; }

    public void UpdateRole(Guid newRoleTypeId)
    {
        RoleTypeId = newRoleTypeId;
        Touch();
    }

    public void SetContribution(decimal percentage)
    {
        if (percentage < 0 || percentage > 100)
            throw new InvalidDomainObjectException("ContributionPercentage", percentage, "Contribution must be 0-100%.");

        ContributionPercentage = percentage;
        Touch();
    }

    public void RecordPayment(decimal amount)
    {
        if (amount < 0)
            throw new ArgumentException("Amount cannot be negative.", nameof(amount));
            
        TotalPaid += amount;
        Touch();
    }

    public void SetAsPrimaryPayer(bool isPrimary)
    {
        IsPrimaryPayer = isPrimary;
        Touch();
    }

    public void Activate()
    {
        IsActive = true;
        Touch();
    }

    public void Deactivate()
    {
        IsActive = false;
        Touch();
    }

    public void RecordActivity(DateTime activityAt)
    {
        LastActivityAt = activityAt;
        Touch();
    }
}
