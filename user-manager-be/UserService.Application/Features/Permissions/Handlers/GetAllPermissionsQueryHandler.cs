using MediatR;
using UserService.Application.Features.Permissions.Queries;
using UserService.Application.Mapping;
using UserService.Domain.Repositories;

namespace UserService.Application.Features.Permissions.Handlers;

public class GetAllPermissionsQueryHandler : IRequestHandler<GetAllPermissionsQuery, ApiResponse<List<PermissionDto>>>
{
    private readonly IPermissionRepository _permissionRepository;

    public GetAllPermissionsQueryHandler(IPermissionRepository permissionRepository)
    {
        _permissionRepository = permissionRepository ?? throw new ArgumentNullException(nameof(permissionRepository));
    }

    public async Task<ApiResponse<List<PermissionDto>>> Handle(GetAllPermissionsQuery request, CancellationToken cancellationToken)
    {
        var permissions = await _permissionRepository.GetActivePermissionsAsync(cancellationToken);
        var dtos = permissions.Select(PermissionEntityMapper.ToDto).ToList();
        return new ApiResponse<List<PermissionDto>>(true, dtos, "OK");
    }
}
