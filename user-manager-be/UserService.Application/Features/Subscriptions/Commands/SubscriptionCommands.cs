using MediatR;
using UserService.Domain.Enums;
using UserService.Application.DTOs.Subscriptions;

namespace UserService.Application.Features.Subscriptions.Commands;

public record ActivateSubscriptionCommand(
    Guid UserId,
    Guid ApplicationId,
    Guid SubscriptionPlanId,
    SubscriptionType SubscriptionType,
    DateTime? ExpiresAt = null,
    int MaxMembers = 1
) : IRequest<SubscriptionDto>;

public record ActivateLicensePackageCommand(
    Guid UserId,
    Guid ApplicationId,
    Guid LicensePackageId,
    SubscriptionType SubscriptionType,
    DateTime? ExpiresAt = null,
    int MaxMembers = 1
) : IRequest<SubscriptionDto>;

public record UpgradeSubscriptionCommand(
    Guid UserId,
    Guid ApplicationId,
    Guid NewSubscriptionPlanId,
    SubscriptionType NewType,
    DateTime? ExpiresAt = null
) : IRequest<SubscriptionDto>;

public record UpgradeLicensePackageCommand(
    Guid UserId,
    Guid ApplicationId,
    Guid NewLicensePackageId,
    SubscriptionType NewType,
    DateTime? ExpiresAt = null
) : IRequest<SubscriptionDto>;

public record ExtendSubscriptionCommand(
    Guid UserId,
    Guid ApplicationId,
    DateTime NewExpiresAt
) : IRequest<SubscriptionDto>;

public record CancelSubscriptionCommand(
    Guid UserId,
    Guid ApplicationId
) : IRequest<Unit>;
