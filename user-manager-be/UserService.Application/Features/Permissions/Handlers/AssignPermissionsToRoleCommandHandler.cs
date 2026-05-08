using MediatR;
using UserService.Application.Features.Permissions.Commands;
using UserService.Application.Services;
using UserService.Domain.Entities;
using UserService.Domain.Repositories;

namespace UserService.Application.Features.Permissions.Handlers;

public class AssignPermissionsToRoleCommandHandler : IRequestHandler<AssignPermissionsToRoleCommand, ApiResponse<SuccessResponse>>
{
    private readonly IPermissionRepository _permissionRepository;
    private readonly IRolePermissionRepository _rolePermissionRepository;
    private readonly ISystemRoleService _roleService;
    private readonly IUnitOfWork _unitOfWork;

    public AssignPermissionsToRoleCommandHandler(
        IPermissionRepository permissionRepository,
        IRolePermissionRepository rolePermissionRepository,
        ISystemRoleService roleService,
        IUnitOfWork unitOfWork)
    {
        _permissionRepository = permissionRepository ?? throw new ArgumentNullException(nameof(permissionRepository));
        _rolePermissionRepository = rolePermissionRepository ?? throw new ArgumentNullException(nameof(rolePermissionRepository));
        _roleService = roleService ?? throw new ArgumentNullException(nameof(roleService));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<ApiResponse<SuccessResponse>> Handle(AssignPermissionsToRoleCommand request, CancellationToken cancellationToken)
    {
        var role = await _roleService.GetRoleByIdAsync(request.RoleId, cancellationToken);
        if (role == null)
        {
            return new ApiResponse<SuccessResponse>(false, null, "Role not found", new ErrorDetail("NOT_FOUND", "Role not found"));
        }

        var permissionIds = request.PermissionIds?.Distinct().ToList() ?? new List<Guid>();
        if (permissionIds.Count == 0)
        {
            return new ApiResponse<SuccessResponse>(true, new SuccessResponse(true, "No permissions to assign"), "OK");
        }

        var added = 0;
        foreach (var permissionId in permissionIds)
        {
            var permission = await _permissionRepository.GetByIdAsync(permissionId, cancellationToken);
            if (permission == null) continue;

            if (await _rolePermissionRepository.ExistsAsync(request.RoleId, permissionId, cancellationToken))
                continue;

            await _rolePermissionRepository.AddAsync(new RolePermission(request.RoleId, permissionId), cancellationToken);
            added++;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new ApiResponse<SuccessResponse>(true, new SuccessResponse(true, $"Assigned {added} permission(s) to role"), "OK");
    }
}
