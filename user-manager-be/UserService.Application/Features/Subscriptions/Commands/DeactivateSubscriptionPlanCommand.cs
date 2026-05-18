using MediatR;

namespace UserService.Application.Features.Subscriptions.Commands;

public record DeactivateSubscriptionPlanCommand(Guid PlanId) : IRequest<Unit>;

