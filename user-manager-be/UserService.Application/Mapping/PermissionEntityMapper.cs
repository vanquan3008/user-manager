using UserService.Application.DTOs.Permissions;
using UserService.Application.DTOs.Roles;
using UserService.Domain.Entities;

namespace UserService.Application.Mapping;

public static class PermissionEntityMapper
{
    public static PermissionDto ToDto(Permission p) => new()
    {
        Id = p.Id,
        Code = p.Code,
        Description = p.Description,
        Category = p.Category,
        IsSystemPermission = p.IsSystemPermission,
        IsActive = p.IsActive,
        Scope = p.Scope,
        CreatedAt = p.CreatedAt
    };

    public static RoleWithPermissionsDto ToRoleWithPermissions(RoleBasicDto role) => new()
    {
        Id = role.Id,
        Name = role.Name,
        Description = role.Description,
        IsSystemRole = role.IsSystemRole,
        Permissions = new List<PermissionDto>()
    };

    public static PermissionWithRolesDto ToPermissionWithRoles(Permission p) => new()
    {
        Id = p.Id,
        Code = p.Code,
        Description = p.Description,
        Category = p.Category,
        AssignedToRoles = new List<RoleDto>()
    };
}
