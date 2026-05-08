using Microsoft.EntityFrameworkCore;
using Serilog;
using UserService.Domain.Constants;
using UserService.Domain.Entities;
using UserService.Infrastructure.Data;
using UserService.Infrastructure.Identity;

namespace UserService.Infrastructure.Seeding;


public class AdminRolePermissionSeeder
{
    private readonly UserDbContext _db;
    private readonly ILogger _log = Log.ForContext<AdminRolePermissionSeeder>();

    public AdminRolePermissionSeeder(UserDbContext db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async Task SeedAsync(CancellationToken ct = default)
    {
        var adminRole = await GetExistingRoleAsync(RoleNames.Admin, ct);
        var saRole = await GetExistingRoleAsync(RoleNames.SuperAdmin, ct);

        if (adminRole is null || saRole is null)
        {
            _log.Warning("AdminRolePermissionSeeder: required roles not found. Ensure SystemRoleSeeder ran first.");
            return;
        }

        var permissions = await _db.Permissions.Where(p => p.IsActive).ToListAsync(ct);
        if (permissions.Count == 0)
        {
            _log.Warning("No permissions found. Run PermissionSeeder first.");
            return;
        }

        var existing = await _db.RolePermissions
            .Where(rp => (rp.RoleId == adminRole.Id || rp.RoleId == saRole.Id))
            .Select(rp => new { rp.RoleId, rp.PermissionId })
            .ToListAsync(ct);

        var existingSet = existing.Select(x => (x.RoleId, x.PermissionId)).ToHashSet();

        foreach (var role in new[] { adminRole, saRole })
        {
            foreach (var perm in permissions)
            {
                if (existingSet.Contains((role.Id, perm.Id)))
                    continue;

                await _db.RolePermissions.AddAsync(new RolePermission(role.Id, perm.Id), ct);
                existingSet.Add((role.Id, perm.Id));
            }
        }

        await _db.SaveChangesAsync(ct);
        _log.Information("Admin/SuperAdmin role-permission seeding completed");
    }

    private async Task<AppRole?> GetExistingRoleAsync(string name, CancellationToken ct)
    {
        var normalized = name.ToUpperInvariant();
        var role = await _db.Set<AppRole>()
            .FirstOrDefaultAsync(r => r.NormalizedName == normalized, ct);

        return role;
    }
}
