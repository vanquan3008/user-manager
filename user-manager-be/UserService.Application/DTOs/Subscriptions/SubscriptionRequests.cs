using UserService.Domain.Enums;

namespace UserService.Application.DTOs.Subscriptions;

public record ActivateSubscriptionRequest(
    Guid? UserId,
    Guid ApplicationId,
    Guid SubscriptionPlanId,
    SubscriptionType SubscriptionType,
    DateTime? ExpiresAt,
    int MaxMembers = 1);

public record ActivateLicensePackageRequest(
    Guid? UserId,
    Guid ApplicationId,
    Guid LicensePackageId,
    SubscriptionType SubscriptionType,
    DateTime? ExpiresAt,
    int MaxMembers = 1);

public record UpgradeSubscriptionRequest(
    Guid? UserId,
    Guid ApplicationId,
    Guid NewSubscriptionPlanId,
    SubscriptionType NewType,
    DateTime? ExpiresAt);

public record UpgradeLicensePackageRequest(
    Guid? UserId,
    Guid ApplicationId,
    Guid NewLicensePackageId,
    SubscriptionType NewType,
    DateTime? ExpiresAt);

public record ExtendSubscriptionRequest(
    Guid? UserId,
    Guid ApplicationId,
    DateTime NewExpiresAt);

public record CancelSubscriptionRequest(
    Guid? UserId,
    Guid ApplicationId);
