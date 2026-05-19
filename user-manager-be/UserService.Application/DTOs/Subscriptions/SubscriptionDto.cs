using UserService.Domain.Enums;

namespace UserService.Application.DTOs.Subscriptions;

public record SubscriptionDto(
    Guid Id,
    Guid UserId,
    Guid ApplicationId,
    Guid? SubscriptionPlanId,
    SubscriptionType SubscriptionType,
    DateTime StartedAt,
    DateTime? ExpiresAt,
    DateTime? CancelledAt,
    bool IsActive,
    int MaxMembers,
    int CurrentMemberCount,
    EntitlementSnapshotDto? EntitlementSnapshot,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
