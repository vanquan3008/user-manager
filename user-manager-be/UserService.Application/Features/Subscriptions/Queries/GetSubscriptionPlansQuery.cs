using MediatR;
using UserService.Application.DTOs.Subscriptions;

namespace UserService.Application.Features.Subscriptions.Queries;

public record GetSubscriptionPlansQuery(Guid? ApplicationId) : IRequest<IReadOnlyList<SubscriptionPlanCatalogDto>>;

