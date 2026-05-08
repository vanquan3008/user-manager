using MediatR;

namespace UserService.Application.Features.Permissions.Queries;

public record GetAllPermissionsQuery()
    : IRequest<ApiResponse<List<PermissionDto>>>;

public record GetPermissionByIdQuery(Guid PermissionId)
    : IRequest<ApiResponse<PermissionDto>>;

public record GetRolePermissionsQuery(Guid RoleId)
    : IRequest<ApiResponse<RoleWithPermissionsDto>>;
