namespace UserService.Application.DTOs.ApplicationRoles;

public record ApplicationRoleDto(
    Guid Id,
    string Name,
    string? Description,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
