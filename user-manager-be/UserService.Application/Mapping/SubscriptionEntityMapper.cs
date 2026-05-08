using UserService.Application.DTOs.Subscriptions;
using UserService.Domain.Entities;
using UserService.Domain.ValueObjects;

namespace UserService.Application.Mapping;

public static class SubscriptionEntityMapper
{
    public static SubscriptionPlanCatalogDto ToCatalogDto(
        SubscriptionPlan plan,
        IReadOnlyDictionary<Guid, string> roleNameById) =>
        new(
            plan.Id,
            plan.ApplicationId,
            plan.Name,
            plan.Description,
            plan.MonthlyPrice,
            plan.YearlyPrice,
            plan.MaxUsers,
            plan.MaxApiCalls,
            plan.IncludeAnalytics,
            plan.IncludeSupport,
            plan.IncludeAdvancedSecurity,
            plan.IncludeCustomDomain,
            plan.IsActive,
            plan.DisplayOrder,
            plan.RoleAssignments.Select(a => a.RoleId).ToList(),
            plan.ToAssignedRolesCsv(roleNameById),
            plan.Icon?.Value);

    public static LicensePackageCatalogDto ToCatalogDto(LicensePackage package) =>
        new(
            package.Id,
            package.ApplicationId,
            package.SubscriptionPlanId,
            package.Name,
            null,
            package.FeatureJson,
            package.SeatLimit,
            package.IsActive,
            package.DisplayOrder,
            package.AssignedRole,
            package.GetFeatures());

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
            s.LicensePackageId,
            s.CreatedAt,
            s.UpdatedAt,
            s.Members?.Select(ToMemberDto).ToList());

    public static SubscriptionMemberDto ToMemberDto(SubscriptionMember m) =>
        new(
            m.Id,
            m.SubscriptionId,
            m.UserId,
            m.User?.Email ?? string.Empty,
            m.User?.Username ?? string.Empty,
            m.RoleType?.Name ?? string.Empty,
            m.IsAdmin,
            m.IsOwner,
            m.IsActive,
            m.JoinedAt,
            m.LastActivityAt);
}
