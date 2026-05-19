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
    IReadOnlyList<string> Features,
    IReadOnlyList<string> Permissions,
    bool IsActive,
    int DisplayOrder,
    string? Icon);

