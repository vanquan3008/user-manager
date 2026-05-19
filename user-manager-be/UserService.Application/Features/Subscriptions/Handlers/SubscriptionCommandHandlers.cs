using MediatR;
using UserService.Application.Features.Subscriptions.Commands;
using UserService.Application.DTOs.Subscriptions;
using UserService.Application.Interfaces.Services;

namespace UserService.Application.Features.Subscriptions.Handlers;

public class ActivateSubscriptionCommandHandler : IRequestHandler<ActivateSubscriptionCommand, SubscriptionDto>
{
    private readonly ISubscriptionManagementService _subscriptionManagementService;

    public ActivateSubscriptionCommandHandler(ISubscriptionManagementService subscriptionManagementService)
    {
        _subscriptionManagementService = subscriptionManagementService;
    }

    public Task<SubscriptionDto> Handle(ActivateSubscriptionCommand command, CancellationToken cancellationToken)
        => _subscriptionManagementService.ActivateAsync(command, cancellationToken);
}

public class UpgradeSubscriptionCommandHandler : IRequestHandler<UpgradeSubscriptionCommand, SubscriptionDto>
{
    private readonly ISubscriptionManagementService _subscriptionManagementService;

    public UpgradeSubscriptionCommandHandler(ISubscriptionManagementService subscriptionManagementService)
    {
        _subscriptionManagementService = subscriptionManagementService;
    }

    public Task<SubscriptionDto> Handle(UpgradeSubscriptionCommand command, CancellationToken cancellationToken)
        => _subscriptionManagementService.UpgradeAsync(command, cancellationToken);
}

public class ExtendSubscriptionCommandHandler : IRequestHandler<ExtendSubscriptionCommand, SubscriptionDto>
{
    private readonly ISubscriptionManagementService _subscriptionManagementService;

    public ExtendSubscriptionCommandHandler(ISubscriptionManagementService subscriptionManagementService)
    {
        _subscriptionManagementService = subscriptionManagementService;
    }

    public Task<SubscriptionDto> Handle(ExtendSubscriptionCommand command, CancellationToken cancellationToken)
        => _subscriptionManagementService.ExtendAsync(command, cancellationToken);
}

public class CancelSubscriptionCommandHandler : IRequestHandler<CancelSubscriptionCommand, Unit>
{
    private readonly ISubscriptionManagementService _subscriptionManagementService;

    public CancelSubscriptionCommandHandler(ISubscriptionManagementService subscriptionManagementService)
    {
        _subscriptionManagementService = subscriptionManagementService;
    }

    public async Task<Unit> Handle(CancelSubscriptionCommand command, CancellationToken cancellationToken)
    {
        await _subscriptionManagementService.CancelAsync(command, cancellationToken);
        return Unit.Value;
    }
}
