namespace UserService.Application.DTOs.Subscriptions;

public record CreateSubscriptionPlanRequest(
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
    string? Icon);

public record CreateLicensePackageRequest(
    Guid ApplicationId,
    Guid SubscriptionPlanId,
    string Name,
    string? Description,
    IReadOnlyList<string> Features,
    int SeatLimit,
    bool IsActive,
    int DisplayOrder,
    string AssignedRole);

public record UpdateLicensePackageRequest(
    Guid ApplicationId,
    string Name,
    string? Description,
    IReadOnlyList<string> Features,
    int SeatLimit,
    bool IsActive,
    int DisplayOrder,
    string AssignedRole);

public record UpdateSubscriptionPlanRequest(
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
    string? Icon);
