using MediatR;
using UserService.Application.DTOs.Subscriptions;
using UserService.Application.Features.Subscriptions.Commands;

namespace UserService.Application.Interfaces.Services;

public interface ISubscriptionManagementService
{
    Task<SubscriptionDto> ActivateAsync(ActivateSubscriptionCommand command, CancellationToken cancellationToken = default);
    Task<SubscriptionDto> UpgradeAsync(UpgradeSubscriptionCommand command, CancellationToken cancellationToken = default);
    Task<SubscriptionDto> ExtendAsync(ExtendSubscriptionCommand command, CancellationToken cancellationToken = default);
    Task CancelAsync(CancelSubscriptionCommand command, CancellationToken cancellationToken = default);
    Task<SubscriptionPlanCatalogDto> CreatePlanAsync(CreateSubscriptionPlanCommand command, CancellationToken cancellationToken = default);
    Task<SubscriptionPlanCatalogDto> UpdatePlanAsync(UpdateSubscriptionPlanCommand command, CancellationToken cancellationToken = default);
    Task DeactivatePlanAsync(DeactivateSubscriptionPlanCommand command, CancellationToken cancellationToken = default);
}
