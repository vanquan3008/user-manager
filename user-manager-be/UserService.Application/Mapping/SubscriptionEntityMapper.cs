using UserService.Application.DTOs.Subscriptions;
using UserService.Domain.Entities;
using UserService.Domain.ValueObjects;

namespace UserService.Application.Mapping;

public static class SubscriptionEntityMapper
{
    public static SubscriptionPlanCatalogDto ToCatalogDto(
        SubscriptionPlan plan,
        IReadOnlyCollection<string> featureCodes,
        IReadOnlyCollection<string> permissionCodes) =>
        new(
            plan.Id,
            plan.ApplicationId,
            plan.Name,
            plan.Description,
            plan.MonthlyPrice,
            plan.YearlyPrice,
            plan.MaxUsers,
            plan.MaxApiCalls,
            featureCodes.ToList(),
            permissionCodes.ToList(),
            plan.IsActive,
            plan.DisplayOrder,
            plan.Icon?.Value);

    public static EntitlementQuotasDto ToDto(EntitlementQuotas q) =>
        new(q.MaxUsers, q.MaxApplications, q.MaxStorageMb, q.MaxMembers, q.MaxApiCalls);

    public static EntitlementPlanMetadataDto? ToDto(EntitlementPlanMetadata? m) =>
        m is null ? null : new(m.PlanId, m.PlanName, m.MonthlyPrice, m.YearlyPrice);

    public static EntitlementSnapshotDto? ToDto(EntitlementSnapshot? snapshot) =>
        snapshot is null
            ? null
            : new(
                snapshot.Features.ToList(),
                ToDto(snapshot.Quotas),
                ToDto(snapshot.PlanMetadata));

    public static SubscriptionDto ToDto(Subscription s) =>
        new(
            s.Id,
            s.UserId,
            s.ApplicationId,
            s.SubscriptionPlanId,
            s.SubscriptionType,
            s.StartedAt,
            s.ExpiresAt,
            s.CancelledAt,
            s.IsActive,
            s.MaxMembers,
            s.CurrentMemberCount,
            ToDto(s.EntitlementSnapshot),
            s.CreatedAt,
            s.UpdatedAt);
}
