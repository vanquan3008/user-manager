namespace UserService.Application.DTOs.ApplicationRoles;

public record CreateApplicationRoleRequest(
    string Name,
    string? Description);
