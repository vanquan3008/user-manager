namespace UserService.Application.DTOs.Applications;

public record CreateApplicationRequest(
    string Name,
    string? Description,
    Guid OwnerId,
    string ClientType = "Public",
    Guid? SubscriptionPlanId = null,
    string? LogoUrl = null,
    string? AllowedRedirectUris = null,
    string? BackchannelLogoutUri = null);
