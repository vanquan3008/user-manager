using Microsoft.EntityFrameworkCore;
using UserService.Domain.Entities;
using UserService.Domain.Repositories;
using UserService.Infrastructure.Data;

namespace UserService.Infrastructure.Repositories;

public class PermissionRepository : Repository<Permission>, IPermissionRepository
{
    private new readonly UserDbContext _context;

    public PermissionRepository(UserDbContext context) : base(context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<IReadOnlyList<string>> GetPermissionCodesByRoleIdsAsync(IEnumerable<Guid> roleIds, CancellationToken cancellationToken = default)
    {
        var idList = roleIds?.Distinct().ToList() ?? new List<Guid>();
        if (idList.Count == 0)
            return Array.Empty<string>();

        return await _context.RolePermissions
            .AsNoTracking()
            .Where(rp => idList.Contains(rp.RoleId))
            .Join(_context.Permissions.Where(p => p.IsActive), rp => rp.PermissionId, p => p.Id, (rp, p) => p.Code.Value)
            .Distinct()
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyDictionary<Guid, string>> GetPermissionCodesCsvByRoleIdsAsync(
    IEnumerable<Guid> roleIds,
    CancellationToken cancellationToken = default)
    {
        var distinctRoleIds = roleIds?
            .Distinct()
            .ToList();

        if (distinctRoleIds == null || distinctRoleIds.Count == 0)
        {
            return new Dictionary<Guid, string>();
        }
        var permissionRows = await _context.RolePermissions
            .AsNoTracking()
            .Where(rp => distinctRoleIds.Contains(rp.RoleId))
            .Join(
                _context.Permissions.AsNoTracking()
                    .Where(p => p.IsActive),
                rolePermission => rolePermission.PermissionId,
                permission => permission.Id,
                (rolePermission, permission) => new
                {
                    RoleId = rolePermission.RoleId,
                    PermissionCode = permission.Code.Value
                })
            .ToListAsync(cancellationToken);

        var result = permissionRows
            .GroupBy(x => x.RoleId)
            .ToDictionary(
                group => group.Key,
                group => string.Join(
                    ",",
                    group
                        .Select(x => x.PermissionCode)
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .OrderBy(code => code, StringComparer.OrdinalIgnoreCase)));

        return result;
    }

    public async Task<IReadOnlyList<Permission>> GetPermissionsByRoleIdAsync(Guid roleId, CancellationToken cancellationToken = default)
    {
        return await _context.RolePermissions
            .AsNoTracking()
            .Where(rp => rp.RoleId == roleId)
            .Join(_context.Permissions.Where(p => p.IsActive), rp => rp.PermissionId, p => p.Id, (rp, p) => p)
            .OrderBy(p => p.Category)
            .ThenBy(p => p.Code)
            .ToListAsync(cancellationToken);
    }

    public async Task<Permission?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(code))
            return null;

        return await _context.Permissions
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Code == code, cancellationToken);
    }

    public async Task<IReadOnlyList<Permission>> GetByCodeAsync(IEnumerable<string> codes, CancellationToken cancellationToken = default)
    {
        var codesList = codes?.ToList() ?? new List<string>();
        if (!codesList.Any())
            return new List<Permission>();

        return await _context.Permissions
            .AsNoTracking()
            .Where(p => codesList.Contains(p.Code))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Permission>> GetByScopeAsync(IEnumerable<string> scopes, CancellationToken cancellationToken = default)
    {
        var scopesList = scopes?
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Select(s => s.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList() ?? new List<string>();

        if (!scopesList.Any())
            return new List<Permission>();

        return await _context.Permissions
            .AsNoTracking()
            .Where(p => p.Scope != null && scopesList.Contains(p.Scope))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Permission>> GetByCategoryAsync(string category, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(category))
            return new List<Permission>();

        return await _context.Permissions
            .AsNoTracking()
            .Where(p => p.Category == category)
            .OrderBy(p => p.Code)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Permission>> GetActivePermissionsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Permissions
            .AsNoTracking()
            .Where(p => p.IsActive)
            .OrderBy(p => p.Category)
            .ThenBy(p => p.Code)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(code))
            return false;
        return await _context.Permissions
            .AsNoTracking()
            .AnyAsync(p => p.Code == code, cancellationToken);
    }
}
