using UserService.Application.DTOs.Roles;

namespace UserService.Application.DTOs.Permissions;

public class PermissionDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public bool IsSystemPermission { get; set; }
    public bool IsActive { get; set; }
    public string? Scope { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class PermissionWithRolesDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public List<RoleDto> AssignedToRoles { get; set; } = new();
}

public class CreatePermissionRequest
{
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string? Scope { get; set; }
}

public class RoleWithPermissionsDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsSystemRole { get; set; }
    public List<PermissionDto> Permissions { get; set; } = new();
}

public class AssignPermissionToRoleRequest
{
    public Guid PermissionId { get; set; }
}

public class AssignPermissionsToRoleRequest
{
    public List<Guid> PermissionIds { get; set; } = new();
}
