using Microsoft.EntityFrameworkCore;
using UserService.Domain.Entities;
using UserService.Domain.Repositories;
using UserService.Domain.ValueObjects;
using UserService.Infrastructure.Data;
using UserService.Infrastructure.Identity;

namespace UserService.Infrastructure.Repositories;

public sealed class ApplicationRoleRepository : IApplicationRoleRepository
{
    private readonly UserDbContext _context;

    public ApplicationRoleRepository(UserDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<IReadOnlyList<(ApplicationRole Mapping, RoleLookup Role)>> GetMappingsWithRolesForApplicationAsync(
        Guid applicationId,
        CancellationToken cancellationToken = default)
    {
        var mappings = await _context.ApplicationRoles
            .AsNoTracking()
            .Where(m => m.ApplicationId == applicationId)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync(cancellationToken);

        if (mappings.Count == 0)
            return Array.Empty<(ApplicationRole, RoleLookup)>();

        var roleIds = mappings.Select(m => m.RoleId).Distinct().ToList();
        var lookups = await _context.Set<AppRole>()
            .AsNoTracking()
            .Where(r => roleIds.Contains(r.Id))
            .Select(r => new RoleLookup(r.Id, r.Name ?? string.Empty, r.Description))
            .ToListAsync(cancellationToken);

        var byId = lookups.ToDictionary(x => x.Id, x => x);
        var result = new List<(ApplicationRole, RoleLookup)>(mappings.Count);
        foreach (var m in mappings)
        {
            if (!byId.TryGetValue(m.RoleId, out var rl))
                continue;
            result.Add((m, rl));
        }

        return result;
    }

    public Task<bool> ExistsAsync(Guid applicationId, Guid roleId, CancellationToken cancellationToken = default)
    {
        return _context.ApplicationRoles.AnyAsync(
            x => x.ApplicationId == applicationId && x.RoleId == roleId,
            cancellationToken);
    }

    public async Task AddAsync(ApplicationRole entity, CancellationToken cancellationToken = default)
    {
        await _context.ApplicationRoles.AddAsync(entity, cancellationToken);
    }

    public async Task<ApplicationRole?> GetTrackedMappingAsync(
        Guid applicationId,
        Guid roleId,
        CancellationToken cancellationToken = default)
    {
        return await _context.ApplicationRoles
            .FirstOrDefaultAsync(m => m.ApplicationId == applicationId && m.RoleId == roleId, cancellationToken);
    }

    public void Remove(ApplicationRole entity)
    {
        _context.ApplicationRoles.Remove(entity);
    }

    public async Task<int> CountRolesLinkedToApplicationAsync(Guid applicationId, IEnumerable<Guid> roleIds, CancellationToken cancellationToken = default)
    {
        var distinct = roleIds.Where(x => x != Guid.Empty).Distinct().ToList();
        if (distinct.Count == 0) return 0;

        return await _context.ApplicationRoles
            .AsNoTracking()
            .Where(ar => ar.ApplicationId == applicationId && distinct.Contains(ar.RoleId))
            .Select(ar => ar.RoleId)
            .Distinct()
            .CountAsync(cancellationToken);
    }
}
