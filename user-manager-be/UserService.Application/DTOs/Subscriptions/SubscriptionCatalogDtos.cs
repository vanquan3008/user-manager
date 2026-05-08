namespace UserService.Application.DTOs.Subscriptions;

public record SubscriptionPlanCatalogDto(
    Guid Id,
    Guid? ApplicationId,
    string Name,
    string Description,
    decimal MonthlyPrice,
    decimal YearlyPrice,
    int MaxUsers,
    int? MaxApiCalls,
    bool IncludeAnalytics,
    bool IncludeSupport,
    bool IncludeAdvancedSecurity,
    bool IncludeCustomDomain,
    bool IsActive,
    int DisplayOrder,
    IReadOnlyList<Guid> AssignedRoleIds,
    string AssignedRole,
    string? Icon);

public record LicensePackageCatalogDto(
    Guid Id,
    Guid ApplicationId,
    Guid? SubscriptionPlanId,
    string Name,
    string? Description,
    string FeatureJson,
    int SeatLimit,
    bool IsActive,
    int DisplayOrder,
    string AssignedRole,
    IReadOnlyList<string> Features);
