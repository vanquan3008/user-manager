using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UserService.Application.Common;
using UserService.Application.Interfaces.Services;
using UserService.Application.Mapping;
using UserService.Domain.Entities;
using UserService.Domain.Repositories;
using UserService.Infrastructure.Identity;

namespace UserService.Infrastructure.Services;

public sealed class SystemRoleService : ISystemRoleService
{
    private readonly RoleManager<AppRole> _roleManager;
    private readonly IPermissionRepository _permissionRepository;
    private readonly IRolePermissionRepository _rolePermissionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SystemRoleService(
        RoleManager<AppRole> roleManager,
        IPermissionRepository permissionRepository,
        IRolePermissionRepository rolePermissionRepository,
        IUnitOfWork unitOfWork)
    {
        _roleManager = roleManager;
        _permissionRepository = permissionRepository;
        _rolePermissionRepository = rolePermissionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<RoleDto>> GetGlobalRolesAsync(CancellationToken cancellationToken = default)
    {
        var roles = await _roleManager.Roles
            .OrderBy(r => r.Name)
            .ToListAsync(cancellationToken);

        var csvByRole = await _permissionRepository
            .GetPermissionCodesCsvByRoleIdsAsync(
                roles.Select(r => r.Id),
                cancellationToken);

        return roles.Select(r => RoleDtoMapper.ToRoleDto(
                r.Id,
                r.Name ?? string.Empty,
                r.Description,
                r.IsSystemRole,
                csvByRole.GetValueOrDefault(r.Id),
                r.CreatedAt,
                r.UpdatedAt))
            .ToList();
    }

    public async Task<RoleBasicDto?> GetRoleByIdAsync(Guid roleId, CancellationToken cancellationToken = default)
    {
        var role = await _roleManager.FindByIdAsync(roleId.ToString());
        return role == null ? null : RoleDtoMapper.ToRoleBasicDto(role.Id, role.Name ?? string.Empty, role.Description, role.IsSystemRole);
    }

    public async Task<RoleDto?> GetGlobalRoleByIdAsync(Guid roleId, CancellationToken cancellationToken = default)
    {
        var role = await _roleManager.FindByIdAsync(roleId.ToString());
        if (role == null)
            return null;
        var csvMap = await _permissionRepository.GetPermissionCodesCsvByRoleIdsAsync(new[] { role.Id }, cancellationToken);
        return RoleDtoMapper.ToRoleDto(role.Id, role.Name ?? string.Empty, role.Description, role.IsSystemRole, csvMap.GetValueOrDefault(role.Id), role.CreatedAt, role.UpdatedAt);
    }

    public async Task<RoleDto?> GetGlobalRoleByNameAsync(string roleName, CancellationToken cancellationToken = default)
    {
        var normalized = roleName.Trim().ToUpperInvariant();
        var role = await Task.Run(() => _roleManager.Roles
            .FirstOrDefault(r => r.NormalizedName == normalized), cancellationToken);

        if (role == null)
            return null;

        var csvMap = await _permissionRepository.GetPermissionCodesCsvByRoleIdsAsync(new[] { role.Id }, cancellationToken);
        return RoleDtoMapper.ToRoleDto(role.Id, role.Name ?? string.Empty, role.Description, role.IsSystemRole, csvMap.GetValueOrDefault(role.Id), role.CreatedAt, role.UpdatedAt);
    }

    public async Task<ApplicationRoleResult> CreateGlobalRoleAsync(string name, string? description, string? permissions = null, CancellationToken cancellationToken = default)
    {
        var exists = await GlobalRoleExistsAsync(name, cancellationToken);
        if (exists)
        {
            return new ApplicationRoleResult(false, new Dictionary<string, string[]>
            {
                ["ROLE_EXISTS"] = new[] { $"Role '{name}' already exists" }
            });
        }

        var normalizedPerms = string.IsNullOrWhiteSpace(permissions) ? null : permissions.Trim();
        var role = new AppRole(name, description ?? string.Empty)
        {
            IsSystemRole = false
        };

        var result = await _roleManager.CreateAsync(role);
        if (!result.Succeeded)
        {
            var errors = result.Errors.ToDictionary(e => e.Code, e => new[] { e.Description });
            return new ApplicationRoleResult(false, errors);
        }

        if (normalizedPerms != null)
        {
            await SyncRolePermissionsFromCodesAsync(role.Id, normalizedPerms, cancellationToken);
        }

        return new ApplicationRoleResult(true);
    }

    public async Task<ApplicationRoleResult> UpdateGlobalRoleAsync(Guid roleId, string? description, string? permissions = null, CancellationToken cancellationToken = default)
    {
        var role = await _roleManager.FindByIdAsync(roleId.ToString());
        if (role == null)
        {
            return new ApplicationRoleResult(false, new Dictionary<string, string[]>
            {
                ["ROLE_NOT_FOUND"] = new[] { "Global role not found" }
            });
        }

        if (description != null)
            role.Description = description.Trim();
        role.UpdatedAt = DateTime.UtcNow;

        var result = await _roleManager.UpdateAsync(role);
        if (!result.Succeeded)
        {
            var errors = result.Errors.ToDictionary(e => e.Code, e => new[] { e.Description });
            return new ApplicationRoleResult(false, errors);
        }

        if (permissions != null)
        {
            await SyncRolePermissionsFromCodesAsync(role.Id, permissions, cancellationToken);
        }

        return new ApplicationRoleResult(true);
    }

    public async Task<ApplicationRoleResult> DeleteGlobalRoleAsync(Guid roleId, CancellationToken cancellationToken = default)
    {
        var role = await _roleManager.FindByIdAsync(roleId.ToString());
        if (role == null)
        {
            return new ApplicationRoleResult(false, new Dictionary<string, string[]>
            {
                ["ROLE_NOT_FOUND"] = new[] { "Global role not found" }
            });
        }

        await _rolePermissionRepository.RemoveAllForRoleAsync(role.Id, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var result = await _roleManager.DeleteAsync(role);
        if (!result.Succeeded)
        {
            var errors = result.Errors.ToDictionary(e => e.Code, e => new[] { e.Description });
            return new ApplicationRoleResult(false, errors);
        }

        return new ApplicationRoleResult(true);
    }

    public async Task<bool> GlobalRoleExistsAsync(string roleName, CancellationToken cancellationToken = default)
    {
        var exists = await Task.Run(() => _roleManager.Roles
            .Any(r => r.Name == roleName), cancellationToken);
        return exists;
    }

    private async Task SyncRolePermissionsFromCodesAsync(Guid roleId, string permissionsCsv, CancellationToken cancellationToken)
    {
        await _rolePermissionRepository.RemoveAllForRoleAsync(roleId, cancellationToken);

        var codes = permissionsCsv
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(c => c.Trim())
            .Where(c => c.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        foreach (var code in codes)
        {
            var perm = await _permissionRepository.GetByCodeAsync(code, cancellationToken);
            if (perm == null)
                continue;

            await _rolePermissionRepository.AddAsync(new RolePermission(roleId, perm.Id), cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
