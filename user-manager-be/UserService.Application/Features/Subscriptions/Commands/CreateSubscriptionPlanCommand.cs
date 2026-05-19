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
    IReadOnlyList<string> Features,
    IReadOnlyList<string>? Permissions,
    bool IsActive,
    int DisplayOrder,
    string? Icon) : IRequest<SubscriptionPlanCatalogDto>;
