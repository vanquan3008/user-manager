namespace UserService.Application.DTOs.Applications;

public record CreateApplicationRequest(
    string Name,
    string? Description,
    Guid OwnerId,
    Guid? DefaultLicensePackageId = null,
    Guid? SubscriptionPlanId = null,
    string? LogoUrl = null,
    string? AllowedRedirectUris = null,
    string? BackchannelLogoutUri = null);
