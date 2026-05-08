using UserService.Domain.Constants;
using UserService.Domain.Exceptions;
using UserService.Domain.ValueObjects;

namespace UserService.Domain.Entities;

public sealed class FamilyPlan : AuditableAggregateRoot<Guid>
{
    private readonly List<FamilyMember> _members = new();
    private readonly List<SharedApplication> _sharedApplications = new();

    private FamilyPlan() : base() { }

    private FamilyPlan(
        Guid id,
        Guid ownerId,
        PlanName familyName,
        DateTime createdAt,
        int maxMembers = 6)
        : base()
    {
        Id = id;
        OwnerId = ownerId;
        FamilyName = familyName ?? throw new ArgumentNullException(nameof(familyName));
        MaxMembers = maxMembers;
        InviteCode = InviteCode.Generate();
        IsActive = true;
        
        MarkCreated();
        _members.Add(FamilyMember.Create(Guid.NewGuid(), ownerId, this.Id, SubscriptionRoleTypeIds.Owner, createdAt));
    }

    public static FamilyPlan Create(
        Guid id,
        Guid ownerId,
        PlanName familyName,
        DateTime createdAt,
        int maxMembers = 6)
    {
        return new FamilyPlan(id, ownerId, familyName, createdAt, maxMembers);
    }

    public Guid OwnerId { get; private set; }
    public User Owner { get; private set; } = null!;

    public PlanName FamilyName { get; private set; } = null!;
    public UrlValue? FamilyIcon { get; private set; }
    public PlanDescription? FamilyDescription { get; private set; }

    public Guid? FamilySubscriptionPlanId { get; private set; }
    public Guid? CurrentFamilySubscriptionId { get; private set; }
    
    public int MaxMembers { get; private set; }
    public int CurrentMemberCount => _members.Count;

    public InviteCode InviteCode { get; private set; } = null!;
    public DateTime? InviteCodeExpiresAt { get; private set; }
    public bool IsInviteCodeActive(DateTime utcNow) => !InviteCodeExpiresAt.HasValue || InviteCodeExpiresAt > utcNow;

    public StorageQuota SharedStorageMB { get; private set; } = StorageQuota.Create(1000);
    public StorageQuota UsedStorageMB { get; private set; } = StorageQuota.Create(0);
    public long AvailableStorageMB => SharedStorageMB.Megabytes - UsedStorageMB.Megabytes;

    public bool CanShareApplications { get; private set; } = true;
    public bool CanShareContent { get; private set; } = true;
    public bool CanSharePayment { get; private set; } = true;

    public IReadOnlyCollection<FamilyMember> Members => _members.AsReadOnly();
    public IReadOnlyCollection<SharedApplication> SharedApplications => _sharedApplications.AsReadOnly();

    public bool IsActive { get; private set; }

    public void UpdateFamilyInfo(
        PlanName familyName,
        UrlValue? familyIcon = null,
        PlanDescription? familyDescription = null)
    {
        FamilyName = familyName ?? throw new ArgumentNullException(nameof(familyName));
        FamilyIcon = familyIcon;
        FamilyDescription = familyDescription;
        Touch();
    }

    public void SetSubscriptionPlan(Guid subscriptionPlanId)
    {
        FamilySubscriptionPlanId = subscriptionPlanId;
        Touch();
    }

    public void InviteMember(Guid userId, Guid roleTypeId, DateTime joinedAt)
    {
        if (_members.Count >= MaxMembers)
            throw new DomainOperationNotAllowedException("InviteMember", "Family is at maximum capacity");

        if (_members.Any(m => m.UserId == userId))
            throw new DomainOperationNotAllowedException("InviteMember", "User is already a family member");

        _members.Add(FamilyMember.Create(Guid.NewGuid(), userId, this.Id, roleTypeId, joinedAt));
        Touch();
    }

    public void RemoveMember(Guid userId)
    {
        var member = _members.FirstOrDefault(m => m.UserId == userId);
        if (member == null)
            throw new DomainOperationNotAllowedException("RemoveMember", "Member not found");

        if (member.IsAdmin && _members.Count(m => m.IsAdmin) <= 1)
            throw new DomainOperationNotAllowedException("RemoveMember", "Cannot remove the last admin");

        if (member.UserId == OwnerId)
            throw new DomainOperationNotAllowedException("RemoveMember", "Cannot remove the owner");

        _members.Remove(member);
        Touch();
    }

    public void PromoteMemberToAdmin(Guid userId)
    {
        var member = _members.FirstOrDefault(m => m.UserId == userId);
        if (member == null)
            throw new DomainOperationNotAllowedException("PromoteMemberToAdmin", "Member not found");

        member.PromoteToAdmin();
        Touch();
    }

    public void DemoteMemberFromAdmin(Guid userId)
    {
        var member = _members.FirstOrDefault(m => m.UserId == userId);
        if (member == null)
            throw new DomainOperationNotAllowedException("DemoteMemberFromAdmin", "Member not found");

        if (_members.Count(m => m.IsAdmin) <= 1)
            throw new DomainOperationNotAllowedException("DemoteMemberFromAdmin", "Must have at least one admin");

        member.DemoteFromAdmin();
        Touch();
    }

    public void ShareApplication(Guid applicationId, IEnumerable<Guid> allowedFamilyMemberIds, DateTime sharedAt)
    {
        if (_sharedApplications.Any(a => a.ApplicationId == applicationId))
            throw new DomainOperationNotAllowedException("ShareApplication", "Application is already shared");

        _sharedApplications.Add(SharedApplication.Create(
            Guid.NewGuid(),
            this.Id,
            applicationId,
            allowedFamilyMemberIds,
            sharedAt));

        Touch();
    }

    public void UpdateSharedApplicationAllowedMembers(Guid applicationId, IEnumerable<Guid> familyMemberIds)
    {
        var sharedApp = _sharedApplications.FirstOrDefault(a => a.ApplicationId == applicationId);
        if (sharedApp == null)
            throw new DomainOperationNotAllowedException("UpdateSharedApplicationAllowedMembers", "Application is not shared");

        sharedApp.UpdateAllowedMembers(familyMemberIds ?? Array.Empty<Guid>());
        Touch();
    }

    public void UnshareApplication(Guid applicationId)
    {
        var sharedApp = _sharedApplications.FirstOrDefault(a => a.ApplicationId == applicationId);
        if (sharedApp == null)
            throw new DomainOperationNotAllowedException("UnshareApplication", "Application is not shared");

        _sharedApplications.Remove(sharedApp);
        Touch();
    }

    public void RegenerateInviteCode(DateTime utcNow, int expiryHours = 24)
    {
        InviteCode = InviteCode.Generate();
        InviteCodeExpiresAt = utcNow.AddHours(expiryHours);
        Touch();
    }

    public void UpdateSharedStorage(StorageQuota storageQuota)
    {
        if (storageQuota.Megabytes < UsedStorageMB.Megabytes)
            throw new DomainOperationNotAllowedException("UpdateSharedStorage", "Cannot reduce storage below current usage");

        SharedStorageMB = storageQuota;
        Touch();
    }

    public void UpdateSharedFeatures(
        bool canShareApplications,
        bool canShareContent,
        bool canSharePayment)
    {
        CanShareApplications = canShareApplications;
        CanShareContent = canShareContent;
        CanSharePayment = canSharePayment;
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

    public void Delete()
    {
        MarkDeleted();
    }
}
