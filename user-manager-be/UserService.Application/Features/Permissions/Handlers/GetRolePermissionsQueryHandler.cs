using MediatR;
using UserService.Application.Features.Permissions.Queries;
using UserService.Application.Mapping;
using UserService.Application.Services;
using UserService.Domain.Repositories;

namespace UserService.Application.Features.Permissions.Handlers;

public class GetRolePermissionsQueryHandler : IRequestHandler<GetRolePermissionsQuery, ApiResponse<RoleWithPermissionsDto>>
{
    private readonly ISystemRoleService _roleService;
    private readonly IPermissionRepository _permissionRepository;

    public GetRolePermissionsQueryHandler(ISystemRoleService roleService, IPermissionRepository permissionRepository)
    {
        _roleService = roleService ?? throw new ArgumentNullException(nameof(roleService));
        _permissionRepository = permissionRepository ?? throw new ArgumentNullException(nameof(permissionRepository));
    }

    public async Task<ApiResponse<RoleWithPermissionsDto>> Handle(GetRolePermissionsQuery request, CancellationToken cancellationToken)
    {
        var role = await _roleService.GetRoleByIdAsync(request.RoleId, cancellationToken);
        if (role == null)
        {
            return new ApiResponse<RoleWithPermissionsDto>(
                false,
                null,
                "Role not found",
                new ErrorDetail("NOT_FOUND", "Role not found"));
        }

        var permissions = await _permissionRepository.GetPermissionsByRoleIdAsync(request.RoleId, cancellationToken);
        var result = PermissionEntityMapper.ToRoleWithPermissions(role);
        result.Permissions = permissions.Select(PermissionEntityMapper.ToDto).ToList();

        return new ApiResponse<RoleWithPermissionsDto>(true, result, "OK");
    }
}
