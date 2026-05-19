using Microsoft.EntityFrameworkCore;
using UserService.Domain.Entities;
using UserService.Domain.Repositories;
using UserService.Infrastructure.Data;

namespace UserService.Infrastructure.Repositories;

public sealed class SubscriptionPlanRepository : Repository<SubscriptionPlan>, ISubscriptionPlanRepository
{
    public SubscriptionPlanRepository(UserDbContext context) : base(context)
    {
    }

    public async Task<SubscriptionPlan?> GetByIdWithCatalogAsync(
        Guid planId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(plan => plan.Features)
            .Include(plan => plan.Permissions)
            .FirstOrDefaultAsync(plan => plan.Id == planId, cancellationToken);
    }

    public async Task<IEnumerable<SubscriptionPlan>> GetByApplicationIdAsync(Guid applicationId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(p => p.ApplicationId == applicationId)
            .OrderBy(p => p.DisplayOrder)
            .ThenBy(p => p.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<SubscriptionPlan>> GetCatalogPlansAsync(Guid? applicationId, CancellationToken cancellationToken = default)
    {
        var q = _dbSet.AsNoTracking().AsQueryable();
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

    public async Task<IReadOnlyDictionary<Guid, IReadOnlyList<string>>> GetFeatureCodesByPlanIdsAsync(
        IEnumerable<Guid> planIds,
        CancellationToken cancellationToken = default)
    {
        var ids = planIds.Where(id => id != Guid.Empty).Distinct().ToList();
        if (ids.Count == 0)
            return new Dictionary<Guid, IReadOnlyList<string>>();

        var rows = await ((UserDbContext)_context).PlanFeatures
            .AsNoTracking()
            .Include(planFeature => planFeature.Feature)
            .Where(planFeature =>
                ids.Contains(planFeature.SubscriptionPlanId) &&
                planFeature.IsEnabled &&
                planFeature.Feature.IsActive)
            .Select(planFeature => new
            {
                planFeature.SubscriptionPlanId,
                planFeature.Feature.Code
            })
            .ToListAsync(cancellationToken);

        return rows
            .GroupBy(row => row.SubscriptionPlanId)
            .ToDictionary(
                group => group.Key,
                group => (IReadOnlyList<string>)group
                    .Select(row => row.Code)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(code => code, StringComparer.OrdinalIgnoreCase)
                    .ToList());
    }

    public async Task<IReadOnlyList<string>> GetFeatureCodesByPlanIdAsync(
        Guid planId,
        CancellationToken cancellationToken = default)
    {
        var byPlan = await GetFeatureCodesByPlanIdsAsync(new[] { planId }, cancellationToken);
        return byPlan.TryGetValue(planId, out var features) ? features : Array.Empty<string>();
    }

    public async Task<IReadOnlyDictionary<Guid, IReadOnlyList<string>>> GetPermissionCodesByPlanIdsAsync(
        IEnumerable<Guid> planIds,
        CancellationToken cancellationToken = default)
    {
        var ids = planIds.Where(id => id != Guid.Empty).Distinct().ToList();
        if (ids.Count == 0)
            return new Dictionary<Guid, IReadOnlyList<string>>();

        var rows = await ((UserDbContext)_context).SubscriptionPlanPermissions
            .AsNoTracking()
            .Include(planPermission => planPermission.Permission)
            .Where(planPermission =>
                ids.Contains(planPermission.SubscriptionPlanId) &&
                planPermission.Permission.IsActive)
            .Select(planPermission => new
            {
                planPermission.SubscriptionPlanId,
                PermissionCode = planPermission.Permission.Code.Value
            })
            .ToListAsync(cancellationToken);

        return rows
            .GroupBy(row => row.SubscriptionPlanId)
            .ToDictionary(
                group => group.Key,
                group => (IReadOnlyList<string>)group
                    .Select(row => row.PermissionCode)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(code => code, StringComparer.OrdinalIgnoreCase)
                    .ToList());
    }

    public async Task<IReadOnlyList<string>> GetPermissionCodesByPlanIdAsync(
        Guid planId,
        CancellationToken cancellationToken = default)
    {
        var byPlan = await GetPermissionCodesByPlanIdsAsync(new[] { planId }, cancellationToken);
        return byPlan.TryGetValue(planId, out var permissions) ? permissions : Array.Empty<string>();
    }

    public async Task<IReadOnlyList<Guid>> ResolveFeatureIdsAsync(
        IEnumerable<string> featureCodes,
        CancellationToken cancellationToken = default)
    {
        var context = (UserDbContext)_context;
        var normalizedCodes = featureCodes
            .Where(code => !string.IsNullOrWhiteSpace(code))
            .Select(code => code.Trim().ToUpperInvariant())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (normalizedCodes.Count == 0)
            return Array.Empty<Guid>();

        var features = await context.Features
            .Where(feature => normalizedCodes.Contains(feature.Code) && feature.IsActive)
            .ToListAsync(cancellationToken);

        var foundCodes = features.Select(feature => feature.Code).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var missingCodes = normalizedCodes.Where(code => !foundCodes.Contains(code)).ToList();
        if (missingCodes.Count > 0)
            throw new InvalidOperationException($"Unknown or inactive feature code(s): {string.Join(", ", missingCodes)}");

        return features
            .Select(feature => feature.Id)
            .Distinct()
            .ToList();
    }

    public async Task<IReadOnlyList<Guid>> ResolvePermissionIdsAsync(
        IEnumerable<string> permissionCodes,
        CancellationToken cancellationToken = default)
    {
        var context = (UserDbContext)_context;
        var normalizedCodes = permissionCodes
            .Where(code => !string.IsNullOrWhiteSpace(code))
            .Select(code => code.Trim().ToUpperInvariant())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (normalizedCodes.Count == 0)
            return Array.Empty<Guid>();

        var permissions = await context.Permissions
            .Where(permission => normalizedCodes.Contains(permission.Code) && permission.IsActive)
            .ToListAsync(cancellationToken);

        var foundCodes = permissions
            .Select(permission => permission.Code.Value)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var missingCodes = normalizedCodes.Where(code => !foundCodes.Contains(code)).ToList();
        if (missingCodes.Count > 0)
            throw new InvalidOperationException($"Unknown or inactive permission code(s): {string.Join(", ", missingCodes)}");

        return permissions
            .Select(permission => permission.Id)
            .Distinct()
            .ToList();
    }
}
