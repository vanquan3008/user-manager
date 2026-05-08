namespace UserService.Application.DTOs.Roles;

public record CreateRoleRequest(
    string Name,
    string? Description,
    string? Permissions = null);

public record UpdateRoleRequest(
    string? Description,
    string? Permissions = null);
