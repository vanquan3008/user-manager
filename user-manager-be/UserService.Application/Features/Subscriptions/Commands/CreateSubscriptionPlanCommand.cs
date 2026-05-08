using MediatR;
using UserService.Application.DTOs.Subscriptions;

namespace UserService.Application.Features.Subscriptions.Commands;

public record CreateSubscriptionPlanCommand(
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
    string? Icon) : IRequest<SubscriptionPlanCatalogDto>;
