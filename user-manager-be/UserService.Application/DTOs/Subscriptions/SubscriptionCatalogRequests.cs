namespace UserService.Application.DTOs.Subscriptions;

public record CreateSubscriptionPlanRequest(
    Guid? ApplicationId,
    string Name,
    string Description,
    decimal MonthlyPrice,
    decimal YearlyPrice,
    int MaxUsers,
    int? MaxApiCalls,
    IReadOnlyList<string> Features,
    IReadOnlyList<string>? Permissions,
    bool IsActive,
    int DisplayOrder,
    string? Icon);

public record UpdateSubscriptionPlanRequest(
    Guid? ApplicationId,
    string Name,
    string Description,
    decimal MonthlyPrice,
    decimal YearlyPrice,
    int MaxUsers,
    int? MaxApiCalls,
    IReadOnlyList<string> Features,
    IReadOnlyList<string>? Permissions,
    bool IsActive,
    int DisplayOrder,
    string? Icon);
