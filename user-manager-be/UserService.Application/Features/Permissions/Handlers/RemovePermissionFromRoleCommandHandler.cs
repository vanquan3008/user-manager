using MediatR;
using UserService.Application.Features.Permissions.Commands;
using UserService.Domain.Repositories;

namespace UserService.Application.Features.Permissions.Handlers;

public class RemovePermissionFromRoleCommandHandler : IRequestHandler<RemovePermissionFromRoleCommand, ApiResponse<SuccessResponse>>
{
    private readonly IRolePermissionRepository _rolePermissionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RemovePermissionFromRoleCommandHandler(IRolePermissionRepository rolePermissionRepository, IUnitOfWork unitOfWork)
    {
        _rolePermissionRepository = rolePermissionRepository ?? throw new ArgumentNullException(nameof(rolePermissionRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<ApiResponse<SuccessResponse>> Handle(RemovePermissionFromRoleCommand request, CancellationToken cancellationToken)
    {
        if (!await _rolePermissionRepository.ExistsAsync(request.RoleId, request.PermissionId, cancellationToken))
        {
            return new ApiResponse<SuccessResponse>(true, new SuccessResponse(true, "Permission was not assigned to role"), "OK");
        }

        await _rolePermissionRepository.RemoveAsync(request.RoleId, request.PermissionId, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new ApiResponse<SuccessResponse>(true, new SuccessResponse(true, "Permission removed from role"), "OK");
    }
}
