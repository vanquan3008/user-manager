using MediatR;
using UserService.Application.Features.Permissions.Queries;
using UserService.Application.Mapping;
using UserService.Domain.Repositories;

namespace UserService.Application.Features.Permissions.Handlers;

public class GetPermissionByIdQueryHandler : IRequestHandler<GetPermissionByIdQuery, ApiResponse<PermissionDto>>
{
    private readonly IPermissionRepository _permissionRepository;

    public GetPermissionByIdQueryHandler(IPermissionRepository permissionRepository)
    {
        _permissionRepository = permissionRepository ?? throw new ArgumentNullException(nameof(permissionRepository));
    }

    public async Task<ApiResponse<PermissionDto>> Handle(GetPermissionByIdQuery request, CancellationToken cancellationToken)
    {
        var permission = await _permissionRepository.GetByIdAsync(request.PermissionId, cancellationToken);
        if (permission == null)
        {
            return new ApiResponse<PermissionDto>(
                false,
                null,
                "Permission not found",
                new ErrorDetail("NOT_FOUND", "Permission not found"));
        }

        var dto = PermissionEntityMapper.ToDto(permission);
        return new ApiResponse<PermissionDto>(true, dto, "OK");
    }
}
