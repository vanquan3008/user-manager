using UserService.Domain.Enums;
using UserService.Domain.Exceptions;

namespace UserService.Domain.Entities;

/// <summary>
/// Canonical SaaS membership: one user in one application with exactly one role.
/// Authorization flags and license assignment must not live here.
/// </summary>
public sealed class ApplicationMembership : AuditableAggregateRoot<Guid>
{
    private ApplicationMembership() : base() { }

    private ApplicationMembership(
        Guid id,
        Guid userId,
        Guid applicationId,
        Guid roleId,
        MembershipStatus status,
        DateTime invitedAt)
        : base()
    {
        if (userId == Guid.Empty)
            throw new InvalidDomainObjectException(nameof(userId), userId, "UserId is required.");
        if (applicationId == Guid.Empty)
            throw new InvalidDomainObjectException(nameof(applicationId), applicationId, "ApplicationId is required.");
        if (roleId == Guid.Empty)
            throw new InvalidDomainObjectException(nameof(roleId), roleId, "RoleId is required.");

        Id = id == Guid.Empty ? Guid.NewGuid() : id;
        UserId = userId;
        ApplicationId = applicationId;
        RoleId = roleId;
        Status = status;
        IsActive = status is MembershipStatus.Invited or MembershipStatus.Active;
        InvitedAt = invitedAt;
        JoinedAt = status == MembershipStatus.Active ? invitedAt : null;
        MarkCreated();
    }

    public static ApplicationMembership Invite(
        Guid id,
        Guid userId,
        Guid applicationId,
        Guid roleId,
        DateTime invitedAt)
        => new(id, userId, applicationId, roleId, MembershipStatus.Invited, invitedAt);

    public static ApplicationMembership Join(
        Guid id,
        Guid userId,
        Guid applicationId,
        Guid roleId,
        DateTime joinedAt)
        => new(id, userId, applicationId, roleId, MembershipStatus.Active, joinedAt);

    public Guid UserId { get; private set; }
    public Guid ApplicationId { get; private set; }
    public Guid RoleId { get; private set; }
    public MembershipStatus Status { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime InvitedAt { get; private set; }
    public DateTime? JoinedAt { get; private set; }

    public User User { get; private set; } = null!;
    public AppApplication Application { get; private set; } = null!;

    public void ChangeRole(Guid roleId)
    {
        if (roleId == Guid.Empty)
            throw new InvalidDomainObjectException(nameof(roleId), roleId, "RoleId is required.");

        RoleId = roleId;
        Touch();
    }

    public void Activate(DateTime joinedAt)
    {
        Status = MembershipStatus.Active;
        IsActive = true;
        JoinedAt ??= joinedAt;
        Touch();
    }

    public void Suspend()
    {
        Status = MembershipStatus.Suspended;
        IsActive = false;
        Touch();
    }

    public void Remove()
    {
        Status = MembershipStatus.Removed;
        IsActive = false;
        MarkDeleted();
    }
}
