namespace UserService.Application.DTOs.Roles;

public record RoleBasicDto(Guid Id, string Name, string? Description, bool IsSystemRole);

public record RoleDto(
    Guid Id,
    string Name,
    string? Description,
    bool IsSystemRole,
    string? Permissions,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
