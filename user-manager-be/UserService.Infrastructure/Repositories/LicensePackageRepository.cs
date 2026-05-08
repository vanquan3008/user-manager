using Microsoft.EntityFrameworkCore;
using UserService.Domain.Entities;
using UserService.Domain.Repositories;
using UserService.Infrastructure.Data;

namespace UserService.Infrastructure.Repositories;

public sealed class LicensePackageRepository : Repository<LicensePackage>, ILicensePackageRepository
{
    public LicensePackageRepository(UserDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<LicensePackage>> GetByApplicationIdAsync(
        Guid applicationId,
        Guid? subscriptionPlanId = null,
        CancellationToken cancellationToken = default)
    {
        var q = _dbSet
            .AsNoTracking()
            .Where(p => p.ApplicationId == applicationId);
        if (subscriptionPlanId.HasValue)
            q = q.Where(p => p.SubscriptionPlanId == subscriptionPlanId);
        return await q
            .OrderBy(p => p.DisplayOrder)
            .ThenBy(p => p.Name)
            .ToListAsync(cancellationToken);
    }
}
