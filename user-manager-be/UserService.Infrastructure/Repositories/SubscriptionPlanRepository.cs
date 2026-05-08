using Microsoft.EntityFrameworkCore;
using UserService.Domain.Entities;
using UserService.Domain.Repositories;
using UserService.Infrastructure.Data;
using UserService.Infrastructure.Identity;

namespace UserService.Infrastructure.Repositories;

public sealed class SubscriptionPlanRepository : Repository<SubscriptionPlan>, ISubscriptionPlanRepository
{
    public SubscriptionPlanRepository(UserDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<SubscriptionPlan>> GetByApplicationIdAsync(Guid applicationId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(p => p.RoleAssignments)
            .Where(p => p.ApplicationId == applicationId)
            .OrderBy(p => p.DisplayOrder)
            .ThenBy(p => p.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<SubscriptionPlan>> GetCatalogPlansAsync(Guid? applicationId, CancellationToken cancellationToken = default)
    {
        var q = _dbSet.AsNoTracking().Include(p => p.RoleAssignments).AsQueryable();
        if (applicationId.HasValue)
        {
            return await q
                .Where(p => p.ApplicationId == applicationId.Value)
                .OrderBy(p => p.DisplayOrder)
                .ThenBy(p => p.Name)
                .ToListAsync(cancellationToken);
        }

        return await q
            .Where(p => !p.ApplicationId.HasValue)
            .OrderBy(p => p.DisplayOrder)
            .ThenBy(p => p.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<SubscriptionPlan?> GetByIdWithRoleAssignmentsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.RoleAssignments)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyDictionary<Guid, string>> GetRoleNamesByIdsAsync(
        IEnumerable<Guid> roleIds,
        CancellationToken cancellationToken = default)
    {
        var ids = roleIds.Where(x => x != Guid.Empty).Distinct().ToList();
        if (ids.Count == 0)
            return new Dictionary<Guid, string>();
        return await _context.Set<AppRole>()
            .AsNoTracking()
            .Where(r => ids.Contains(r.Id))
            .ToDictionaryAsync(r => r.Id, r => r.Name ?? string.Empty, cancellationToken);
    }
}
