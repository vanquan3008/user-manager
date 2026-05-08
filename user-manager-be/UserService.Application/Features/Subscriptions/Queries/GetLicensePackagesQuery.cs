using MediatR;
using UserService.Application.DTOs.Subscriptions;

namespace UserService.Application.Features.Subscriptions.Queries;

public record GetLicensePackagesQuery(Guid ApplicationId, Guid? SubscriptionPlanId = null) : IRequest<IReadOnlyList<LicensePackageCatalogDto>>;

