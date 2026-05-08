using MediatR;
using UserService.Application.DTOs.Subscriptions;
using UserService.Application.Features.Subscriptions.Queries;
using UserService.Application.Mapping;
using UserService.Domain.Repositories;

namespace UserService.Application.Features.Subscriptions.Handlers;

public class GetLicensePackagesQueryHandler : IRequestHandler<GetLicensePackagesQuery, IReadOnlyList<LicensePackageCatalogDto>>
{
    private readonly ILicensePackageRepository _packageRepository;

    public GetLicensePackagesQueryHandler(ILicensePackageRepository packageRepository)
    {
        _packageRepository = packageRepository;
    }

    public async Task<IReadOnlyList<LicensePackageCatalogDto>> Handle(GetLicensePackagesQuery request, CancellationToken cancellationToken)
    {
        var packages = await _packageRepository.GetByApplicationIdAsync(
            request.ApplicationId,
            request.SubscriptionPlanId,
            cancellationToken);
        return packages.Select(SubscriptionEntityMapper.ToCatalogDto).ToList();
    }
}
