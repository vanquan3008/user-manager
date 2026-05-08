using UserService.Domain.Constants;
using UserService.Domain.Enums;
using UserService.Domain.Events;
using UserService.Domain.Exceptions;
using UserService.Domain.ValueObjects;

namespace UserService.Domain.Entities;

public sealed class Subscription : AuditableAggregateRoot<Guid>
{
    private readonly List<SubscriptionMember> _members = new();

    private Subscription() : base() { }

    private Subscription(
        Guid id,
        Guid userId,
        Guid applicationId,
        Guid? subscriptionPlanId,
        Guid? licensePackageId,
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
        LicensePackageId = licensePackageId;
        SubscriptionType = type;
        MaxMembers = maxMembers;
        _entitlementSnapshot = entitlementSnapshot;
        StartedAt = startedAt;
        
        MarkCreated();
        _members.Add(SubscriptionMember.Create(Guid.NewGuid(), id, userId, SubscriptionRoleTypeIds.Owner, startedAt));

        if (type != SubscriptionType.Free)
            AddDomainEvent(new SubscriptionActivatedEvent(id, userId, type));
    }

    public static Subscription AssignLicense(
        Guid id,
        Guid userId,
        Guid applicationId,
        LicensePackage package,
        SubscriptionType type,
        DateTime startedAt,
        int maxMembers = 1)
    {
        if (package == null)
            throw new ArgumentNullException(nameof(package));
        if (package.ApplicationId != applicationId)
            throw new InvalidDomainObjectException("ApplicationId", applicationId, "License package must belong to the same application.");

        var snapshot = package.CreateEntitlementSnapshot();
        var quotas = new EntitlementQuotas(
            snapshot.Quotas.MaxUsers,
            snapshot.Quotas.MaxApplications,
            snapshot.Quotas.MaxStorageMb,
            maxMembers,
            snapshot.Quotas.MaxApiCalls);
        var snapshotWithMembers = new EntitlementSnapshot(snapshot.Features, quotas, snapshot.PlanMetadata);
        return new Subscription(id, userId, applicationId, null, package.Id, type, startedAt, maxMembers, snapshotWithMembers);
    }

    public static Subscription AssignPlan(
        Guid id,
        Guid userId,
        Guid applicationId,
        SubscriptionPlan plan,
        SubscriptionType type,
        DateTime startedAt,
        int maxMembers = 1)
    {
        if (plan == null)
            throw new ArgumentNullException(nameof(plan));
        if (!plan.ApplicationId.HasValue || plan.ApplicationId.Value != applicationId)
            throw new InvalidDomainObjectException("ApplicationId", applicationId, "Plan must belong to the same application.");

        var snapshot = plan.CreateEntitlementSnapshot(maxMembers);
        return new Subscription(id, userId, applicationId, plan.Id, null, type, startedAt, maxMembers, snapshot);
    }

    public Guid UserId { get; private set; }
    public Guid ApplicationId { get; private set; }
    public Guid? SubscriptionPlanId { get; private set; }
    public Guid? LicensePackageId { get; private set; }
    
    private EntitlementSnapshot? _entitlementSnapshot;
    public EntitlementSnapshot? EntitlementSnapshot => _entitlementSnapshot;

    public User User { get; private set; } = null!;
    public AppApplication Application { get; private set; } = null!;
    public SubscriptionPlan? SubscriptionPlan { get; private set; }
    public LicensePackage? LicensePackage { get; private set; }

    public SubscriptionType SubscriptionType { get; private set; }
    public DateTime StartedAt { get; private set; }

    public int MaxMembers { get; private set; }
    public int CurrentMemberCount => _members.Count;
    public IReadOnlyCollection<SubscriptionMember> Members => _members.AsReadOnly();

    public DateTime? ExpiresAt { get; private set; }
    public DateTime? CancelledAt { get; private set; }
    
    public bool IsActive { get; private set; } = true;
    public bool IsExpired(DateTime utcNow) => ExpiresAt.HasValue && ExpiresAt <= utcNow && !IsActive;

    public void UpgradeSubscription(
        SubscriptionType newType,
        Guid? newSubscriptionPlanId,
        Guid? newLicensePackageId,
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
        if (newLicensePackageId.HasValue) LicensePackageId = newLicensePackageId;
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

    public void AddMember(Guid userId, Guid roleTypeId, DateTime joinedAt)
    {
        if (_members.Count >= MaxMembers)
            throw new DomainOperationNotAllowedException("AddMember", $"Subscription has reached max members limit ({MaxMembers})");

        if (_members.Any(m => m.UserId == userId))
            throw new DomainOperationNotAllowedException("AddMember", "User is already a member of this subscription");

        _members.Add(SubscriptionMember.Create(Guid.NewGuid(), Id, userId, roleTypeId, joinedAt));
        Touch();

        AddDomainEvent(new SubscriptionMemberAddedEvent(Id, UserId, userId, roleTypeId));
    }

    public void RemoveMember(Guid userId)
    {
        var member = _members.FirstOrDefault(m => m.UserId == userId);
        if (member == null)
            throw new DomainOperationNotAllowedException("RemoveMember", "Member not found");

        if (member.IsOwner)
            throw new DomainOperationNotAllowedException("RemoveMember", "Cannot remove subscription owner");

        if (_members.Count == 1)
            throw new DomainOperationNotAllowedException("RemoveMember", "Cannot remove the last member");

        _members.Remove(member);
        Touch();

        AddDomainEvent(new SubscriptionMemberRemovedEvent(Id, UserId, userId));
    }

    public void UpdateMemberRole(Guid userId, Guid newRoleTypeId)
    {
        var member = _members.FirstOrDefault(m => m.UserId == userId);
        if (member == null)
            throw new DomainOperationNotAllowedException("UpdateMemberRole", "Member not found");

        if (member.IsOwner && newRoleTypeId != SubscriptionRoleTypeIds.Owner)
            throw new DomainOperationNotAllowedException("UpdateMemberRole", "Cannot change owner role");

        member.UpdateRole(newRoleTypeId);
        Touch();

        AddDomainEvent(new SubscriptionMemberRoleChangedEvent(Id, userId, newRoleTypeId));
    }

    public void SetMemberContribution(Guid userId, decimal percentage)
    {
        var member = _members.FirstOrDefault(m => m.UserId == userId);
        if (member == null)
            throw new DomainOperationNotAllowedException("SetMemberContribution", "Member not found");

        member.SetContribution(percentage);
        Touch();
    }

    public SubscriptionMember? GetMember(Guid userId)
    {
        return _members.FirstOrDefault(m => m.UserId == userId);
    }

    public IEnumerable<SubscriptionMember> GetAdmins()
    {
        return _members.Where(m => m.IsAdmin && m.IsActive);
    }

    public bool CanUserManageMembers(Guid userId)
    {
        var member = GetMember(userId);
        return member?.IsAdmin ?? false;
    }

    public bool CanUserChangePayment(Guid userId)
    {
        var member = GetMember(userId);
        return member?.IsAdmin ?? false;
    }

    public bool CanUserAccessAllApps(Guid userId)
    {
        var member = GetMember(userId);
        return member != null && member.IsActive;
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
