using MediatR;
using UserService.Application.Features.Permissions.Commands;
using UserService.Application.Services;
using UserService.Domain.Entities;
using UserService.Domain.Repositories;

namespace UserService.Application.Features.Permissions.Handlers;

public class AssignPermissionToRoleCommandHandler : IRequestHandler<AssignPermissionToRoleCommand, ApiResponse<SuccessResponse>>
{
    private readonly IPermissionRepository _permissionRepository;
    private readonly IRolePermissionRepository _rolePermissionRepository;
    private readonly ISystemRoleService _roleService;
    private readonly IUnitOfWork _unitOfWork;

    public AssignPermissionToRoleCommandHandler(
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

    public async Task<ApiResponse<SuccessResponse>> Handle(AssignPermissionToRoleCommand request, CancellationToken cancellationToken)
    {
        var role = await _roleService.GetRoleByIdAsync(request.RoleId, cancellationToken);
        if (role == null)
        {
            return new ApiResponse<SuccessResponse>(false, null, "Role not found", new ErrorDetail("NOT_FOUND", "Role not found"));
        }

        var permission = await _permissionRepository.GetByIdAsync(request.PermissionId, cancellationToken);
        if (permission == null)
        {
            return new ApiResponse<SuccessResponse>(false, null, "Permission not found", new ErrorDetail("NOT_FOUND", "Permission not found"));
        }

        if (await _rolePermissionRepository.ExistsAsync(request.RoleId, request.PermissionId, cancellationToken))
        {
            return new ApiResponse<SuccessResponse>(true, new SuccessResponse(true, "Permission already assigned"), "OK");
        }

        await _rolePermissionRepository.AddAsync(new RolePermission(request.RoleId, request.PermissionId), cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new ApiResponse<SuccessResponse>(true, new SuccessResponse(true, $"Permission '{permission.Code}' assigned to role"), "OK");
    }
}
