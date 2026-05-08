using Microsoft.EntityFrameworkCore;
using UserService.Domain.Entities;
using UserService.Domain.Repositories;
using UserService.Infrastructure.Data;

namespace UserService.Infrastructure.Repositories;

public class RolePermissionRepository : IRolePermissionRepository
{
    private readonly UserDbContext _context;

    public RolePermissionRepository(UserDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<bool> ExistsAsync(Guid roleId, Guid permissionId, CancellationToken cancellationToken = default)
    {
        return await _context.RolePermissions
            .AnyAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId, cancellationToken);
    }

    public async Task AddAsync(RolePermission rolePermission, CancellationToken cancellationToken = default)
    {
        await _context.RolePermissions.AddAsync(rolePermission, cancellationToken);
    }

    public async Task RemoveAsync(Guid roleId, Guid permissionId, CancellationToken cancellationToken = default)
    {
        var rp = await _context.RolePermissions
            .FirstOrDefaultAsync(r => r.RoleId == roleId && r.PermissionId == permissionId, cancellationToken);
        if (rp != null)
            _context.RolePermissions.Remove(rp);
    }

    public async Task RemoveAllForRoleAsync(Guid roleId, CancellationToken cancellationToken = default)
    {
        await _context.RolePermissions
            .Where(rp => rp.RoleId == roleId)
            .ExecuteDeleteAsync(cancellationToken);
    }
}
