using MediatR;
using UserService.Application.Features.Subscriptions.Commands;
using UserService.Application.Interfaces.Services;

namespace UserService.Application.Features.Subscriptions.Handlers;

public sealed class DeactivateSubscriptionPlanCommandHandler : IRequestHandler<DeactivateSubscriptionPlanCommand, Unit>
{
    private readonly ISubscriptionManagementService _subscriptionManagementService;

    public DeactivateSubscriptionPlanCommandHandler(ISubscriptionManagementService subscriptionManagementService)
    {
        _subscriptionManagementService = subscriptionManagementService;
    }

    public async Task<Unit> Handle(DeactivateSubscriptionPlanCommand request, CancellationToken cancellationToken)
    {
        await _subscriptionManagementService.DeactivatePlanAsync(request, cancellationToken);
        return Unit.Value;
    }
}

