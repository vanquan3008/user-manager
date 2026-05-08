using UserService.Domain.Entities;

namespace UserService.Domain.Repositories;

public interface ILicensePackageRepository : IRepository<LicensePackage>
{
    Task<IEnumerable<LicensePackage>> GetByApplicationIdAsync(
        Guid applicationId,
        Guid? subscriptionPlanId = null,
        CancellationToken cancellationToken = default);
}
