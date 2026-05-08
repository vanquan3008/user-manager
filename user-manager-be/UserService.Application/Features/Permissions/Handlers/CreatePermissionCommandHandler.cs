using MediatR;
using UserService.Application.Features.Permissions.Commands;
using UserService.Application.Mapping;
using UserService.Domain.Entities;
using UserService.Domain.Repositories;
using UserService.Domain.ValueObjects;

namespace UserService.Application.Features.Permissions.Handlers;

public class CreatePermissionCommandHandler : IRequestHandler<CreatePermissionCommand, ApiResponse<PermissionDto>>
{
    private readonly IPermissionRepository _permissionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreatePermissionCommandHandler(IPermissionRepository permissionRepository, IUnitOfWork unitOfWork)
    {
        _permissionRepository = permissionRepository ?? throw new ArgumentNullException(nameof(permissionRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<ApiResponse<PermissionDto>> Handle(CreatePermissionCommand request, CancellationToken cancellationToken)
    {
        if (await _permissionRepository.ExistsByCodeAsync(request.Code, cancellationToken))
        {
            return new ApiResponse<PermissionDto>(
                false,
                null,
                "Permission with this code already exists",
                new ErrorDetail("DUPLICATE_PERMISSION", $"Permission code '{request.Code}' already exists"));
        }

        var permission = Permission.Create(
            PermissionCode.Create(request.Code),
            request.Description,
            request.Category,
            isSystemPermission: false);

        if (!string.IsNullOrWhiteSpace(request.Scope))
        {
            permission.SetScope(request.Scope);
        }

        await _permissionRepository.AddAsync(permission, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = PermissionEntityMapper.ToDto(permission);
        return new ApiResponse<PermissionDto>(true, dto, "Permission created successfully");
    }
}
