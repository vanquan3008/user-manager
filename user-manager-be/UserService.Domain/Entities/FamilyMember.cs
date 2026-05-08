using UserService.Domain.Constants;
using UserService.Domain.Exceptions;

namespace UserService.Domain.Entities;

public sealed class FamilyMember : Entity<Guid>
{
    private FamilyMember() : base() { }
    
    private FamilyMember(
        Guid id,
        Guid userId,
        Guid familyPlanId,
        Guid roleTypeId,
        DateTime joinedAt)
        : base()
    {
        Id = id;
        UserId = userId;
        FamilyPlanId = familyPlanId;
        RoleTypeId = roleTypeId;
        IsActive = true;
        JoinedAt = joinedAt;
        MarkCreated();
    }

    public static FamilyMember Create(
        Guid id,
        Guid userId,
        Guid familyPlanId,
        Guid roleTypeId,
        DateTime joinedAt)
    {
        return new FamilyMember(id, userId, familyPlanId, roleTypeId, joinedAt);
    }

    public Guid UserId { get; private set; }
    public Guid FamilyPlanId { get; private set; }
    public Guid RoleTypeId { get; private set; }

    public User User { get; private set; } = null!;
    public FamilyPlan FamilyPlan { get; private set; } = null!;
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
    public DateTime JoinedAt { get; private set; }
    public DateTime? LastActivityAt { get; private set; }

    public void PromoteToAdmin()
    {
        RoleTypeId = SubscriptionRoleTypeIds.Admin;
        Touch();
    }

    public void DemoteFromAdmin()
    {
        if (IsOwner)
            throw new DomainOperationNotAllowedException("DemoteFromAdmin", "Cannot demote Owner");

        RoleTypeId = SubscriptionRoleTypeIds.Member;
        Touch();
    }

    public void Activate(DateTime activatedAt)
    {
        IsActive = true;
        if (JoinedAt == default)
            JoinedAt = activatedAt;
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
