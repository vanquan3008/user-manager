using MediatR;
using UserService.Application.DTOs.Subscriptions;
using UserService.Application.Features.Subscriptions.Commands;
using UserService.Application.Interfaces.Services;

namespace UserService.Application.Features.Subscriptions.Handlers;

public class CreateSubscriptionPlanCommandHandler : IRequestHandler<CreateSubscriptionPlanCommand, SubscriptionPlanCatalogDto>
{
    private readonly ISubscriptionManagementService _subscriptionManagementService;

    public CreateSubscriptionPlanCommandHandler(ISubscriptionManagementService subscriptionManagementService)
    {
        _subscriptionManagementService = subscriptionManagementService;
    }

    public Task<SubscriptionPlanCatalogDto> Handle(CreateSubscriptionPlanCommand command, CancellationToken cancellationToken)
        => _subscriptionManagementService.CreatePlanAsync(command, cancellationToken);
}
