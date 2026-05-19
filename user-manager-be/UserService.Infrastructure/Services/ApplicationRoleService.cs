using Microsoft.AspNetCore.Identity;
using UserService.Application.Common;
using UserService.Application.Interfaces.Services;
using UserService.Application.Mapping;
using UserService.Domain.Constants;
using UserService.Domain.Entities;
using UserService.Domain.Repositories;
using UserService.Infrastructure.Identity;

namespace UserService.Infrastructure.Services;

public sealed class ApplicationRoleService : IApplicationRoleService
{
    private readonly RoleManager<AppRole> _roleManager;
    private readonly IApplicationRoleRepository _applicationRoles;
    private readonly IUnitOfWork _unitOfWork;

    public ApplicationRoleService(
        RoleManager<AppRole> roleManager,
        IApplicationRoleRepository applicationRoles,
        IUnitOfWork unitOfWork)
    {
        _roleManager = roleManager;
        _applicationRoles = applicationRoles ?? throw new ArgumentNullException(nameof(applicationRoles));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<IReadOnlyList<ApplicationRoleDto>> GetRolesForApplicationAsync(Guid applicationId, CancellationToken cancellationToken = default)
    {
        var rows = await _applicationRoles.GetMappingsWithRolesForApplicationAsync(applicationId, cancellationToken);
        if (rows.Count == 0)
            return Array.Empty<ApplicationRoleDto>();

        return rows.Select(r => RoleDtoMapper.ToApplicationRoleDto(r.Role, r.Mapping)).ToList();
    }

    public async Task<bool> RoleExistsForApplicationAsync(Guid applicationId, string roleName, CancellationToken cancellationToken = default)
    {
        var trimmedRoleName = roleName?.Trim();
        if (string.IsNullOrWhiteSpace(trimmedRoleName))
            return false;

        var role = await _roleManager.FindByNameAsync(trimmedRoleName);
        if (role is null)
            return false;

        return await _applicationRoles.ExistsAsync(applicationId, role.Id, cancellationToken);
    }

    public async Task<ApplicationRoleResult> CreateRoleAsync(Guid applicationId, string name, string? description, CancellationToken cancellationToken = default)
    {
        var trimmedName = name?.Trim();
        if (string.IsNullOrWhiteSpace(trimmedName))
        {
            return new ApplicationRoleResult(false, new Dictionary<string, string[]>
            {
                ["ROLE_INVALID"] = new[] { "Role name is required." }
            });
        }

        var trimmedDescription = (description ?? string.Empty).Trim();
        var desiredIsSystemRole = RoleNames.SystemRoles.Contains(trimmedName, StringComparer.OrdinalIgnoreCase);
        var role = await _roleManager.FindByNameAsync(trimmedName);
        if (role is null)
        {
            return new ApplicationRoleResult(false, new Dictionary<string, string[]>
            {
                ["ROLE_NOT_FOUND"] = new[] { "Global role does not exist. Please create it in global roles first." }
            });
        }

        var newDescription = string.IsNullOrWhiteSpace(trimmedDescription) ? role.Description : trimmedDescription;
        var descriptionChanged = role.Description != newDescription;
        var isSystemRoleChanged = role.IsSystemRole != desiredIsSystemRole;

        if (descriptionChanged || isSystemRoleChanged)
        {
            role.Description = newDescription;
            role.IsSystemRole = desiredIsSystemRole;
            role.UpdatedAt = DateTime.UtcNow;
            var updateResult = await _roleManager.UpdateAsync(role);
            if (!updateResult.Succeeded)
            {
                var errors = updateResult.Errors.ToDictionary(e => e.Code, e => new[] { e.Description });
                return new ApplicationRoleResult(false, errors);
            }
        }

        var mappingExists = await _applicationRoles.ExistsAsync(applicationId, role.Id, cancellationToken);

        if (!mappingExists)
        {
            await _applicationRoles.AddAsync(new ApplicationRole(applicationId, role.Id), cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return new ApplicationRoleResult(true);
    }

    public async Task<ApplicationRoleResult> UpdateRoleAsync(Guid applicationId, string roleName, string? description, CancellationToken cancellationToken = default)
    {
        var trimmedRoleName = roleName?.Trim();
        if (string.IsNullOrWhiteSpace(trimmedRoleName))
        {
            return new ApplicationRoleResult(false, new Dictionary<string, string[]>
            {
                ["ROLE_INVALID"] = new[] { "Role name is required." }
            });
        }

        var role = await _roleManager.FindByNameAsync(trimmedRoleName);
        if (role is null)
        {
            return new ApplicationRoleResult(false, new Dictionary<string, string[]>
            {
                ["ROLE_NOT_FOUND"] = new[] { "Role not found for this application" }
            });
        }

        var mappingExists = await _applicationRoles.ExistsAsync(applicationId, role.Id, cancellationToken);

        if (!mappingExists)
        {
            return new ApplicationRoleResult(false, new Dictionary<string, string[]>
            {
                ["ROLE_NOT_FOUND"] = new[] { "Role not found for this application" }
            });
        }

        var desiredIsSystemRole = RoleNames.SystemRoles.Contains(trimmedRoleName, StringComparer.OrdinalIgnoreCase);

        role.Description = description?.Trim() ?? role.Description;
        role.IsSystemRole = desiredIsSystemRole;
        role.UpdatedAt = DateTime.UtcNow;

        var result = await _roleManager.UpdateAsync(role);
        if (!result.Succeeded)
        {
            var errors = result.Errors.ToDictionary(e => e.Code, e => new[] { e.Description });
            return new ApplicationRoleResult(false, errors);
        }

        return new ApplicationRoleResult(true);
    }

    public async Task<ApplicationRoleResult> DeleteRoleAsync(Guid applicationId, string roleName, CancellationToken cancellationToken = default)
    {
        var trimmedRoleName = roleName?.Trim();
        if (string.IsNullOrWhiteSpace(trimmedRoleName))
        {
            return new ApplicationRoleResult(false, new Dictionary<string, string[]>
            {
                ["ROLE_INVALID"] = new[] { "Role name is required." }
            });
        }

        var role = await _roleManager.FindByNameAsync(trimmedRoleName);
        if (role is null)
        {
            return new ApplicationRoleResult(false, new Dictionary<string, string[]>
            {
                ["ROLE_NOT_FOUND"] = new[] { "Role not found for this application" }
            });
        }

        var mapping = await _applicationRoles.GetTrackedMappingAsync(applicationId, role.Id, cancellationToken);

        if (mapping is null)
        {
            return new ApplicationRoleResult(false, new Dictionary<string, string[]>
            {
                ["ROLE_NOT_FOUND"] = new[] { "Role not found for this application" }
            });
        }

        _applicationRoles.Remove(mapping);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new ApplicationRoleResult(true);
    }

    public async Task<bool> RoleExistsForUserAssignmentAsync(Guid applicationId, string roleName, CancellationToken cancellationToken = default)
    {
        var trimmedRoleName = roleName?.Trim();
        if (string.IsNullOrWhiteSpace(trimmedRoleName))
            return false;

        var role = await _roleManager.FindByNameAsync(trimmedRoleName);
        if (role is null)
            return false;

        return await _applicationRoles.ExistsAsync(applicationId, role.Id, cancellationToken);
    }

    public async Task<Guid?> GetRoleIdForUserAssignmentAsync(Guid applicationId, string roleName, CancellationToken cancellationToken = default)
    {
        var trimmedRoleName = roleName?.Trim();
        if (string.IsNullOrWhiteSpace(trimmedRoleName))
            return null;

        var role = await _roleManager.FindByNameAsync(trimmedRoleName);
        if (role is null)
            return null;

        var exists = await _applicationRoles.ExistsAsync(applicationId, role.Id, cancellationToken);
        return exists ? role.Id : null;
    }
}
