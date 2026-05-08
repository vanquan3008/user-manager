using MediatR;

namespace UserService.Application.Features.Permissions.Commands;

public record CreatePermissionCommand(
    string Code,
    string Description,
    string Category,
    string? Scope = null)
    : IRequest<ApiResponse<PermissionDto>>;

public record AssignPermissionToRoleCommand(
    Guid RoleId,
    Guid PermissionId)
    : IRequest<ApiResponse<SuccessResponse>>;

public record AssignPermissionsToRoleCommand(
    Guid RoleId,
    IEnumerable<Guid> PermissionIds)
    : IRequest<ApiResponse<SuccessResponse>>;

public record RemovePermissionFromRoleCommand(
    Guid RoleId,
    Guid PermissionId)
    : IRequest<ApiResponse<SuccessResponse>>;
