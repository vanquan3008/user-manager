using MediatR;
using UserService.Application.DTOs.Subscriptions;
using UserService.Application.Features.Subscriptions.Commands;
using UserService.Application.Interfaces.Services;

namespace UserService.Application.Features.Subscriptions.Handlers;

public class UpdateSubscriptionPlanCommandHandler : IRequestHandler<UpdateSubscriptionPlanCommand, SubscriptionPlanCatalogDto>
{
    private readonly ISubscriptionManagementService _subscriptionManagementService;

    public UpdateSubscriptionPlanCommandHandler(ISubscriptionManagementService subscriptionManagementService)
    {
        _subscriptionManagementService = subscriptionManagementService;
    }

    public Task<SubscriptionPlanCatalogDto> Handle(UpdateSubscriptionPlanCommand command, CancellationToken cancellationToken)
        => _subscriptionManagementService.UpdatePlanAsync(command, cancellationToken);
}
