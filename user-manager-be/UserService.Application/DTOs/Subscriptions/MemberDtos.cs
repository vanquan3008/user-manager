namespace UserService.Application.DTOs.Subscriptions;

public record SubscriptionMemberDto(
    Guid Id,
    Guid SubscriptionId,
    Guid UserId,
    string UserEmail,
    string UserName,
    string RoleName,
    bool IsAdmin,
    bool IsOwner,
    bool IsActive,
    DateTime JoinedAt,
    DateTime? LastActivityAt);

public record AddSubscriptionMemberRequest(
    Guid SubscriptionId,
    Guid ApplicationId,
    Guid UserId,
    Guid RoleTypeId);

public record RemoveSubscriptionMemberRequest(
    Guid SubscriptionId,
    Guid ApplicationId,
    Guid UserId);
