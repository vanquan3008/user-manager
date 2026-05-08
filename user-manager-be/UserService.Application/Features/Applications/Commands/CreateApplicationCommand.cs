using MediatR;

namespace UserService.Application.Features.Applications.Commands;

public record CreateApplicationCommand(
    string Name,
    string? Description,
    Guid OwnerId,
    Guid? DefaultLicensePackageId = null,
    Guid? SubscriptionPlanId = null,
    string? LogoUrl = null,
    string? AllowedRedirectUris = null,
    string? BackchannelLogoutUri = null
) : IRequest<CreateApplicationResponse>;
