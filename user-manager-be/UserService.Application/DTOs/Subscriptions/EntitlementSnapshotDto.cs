namespace UserService.Application.DTOs.Subscriptions;

public record EntitlementSnapshotDto(
    IReadOnlyList<string> Features,
    EntitlementQuotasDto Quotas,
    EntitlementPlanMetadataDto? PlanMetadata);

public record EntitlementQuotasDto(
    int MaxUsers,
    int MaxApplications,
    int MaxStorageMb,
    int MaxMembers,
    int? MaxApiCalls);

public record EntitlementPlanMetadataDto(
    Guid PlanId,
    string PlanName,
    decimal? MonthlyPrice,
    decimal? YearlyPrice);
