using MediatR;

namespace UserService.Application.Features.Roles.Queries;

/// <summary>
/// Query to get all global/system roles.
/// </summary>
public record GetRolesQuery : IRequest<IReadOnlyList<RoleDto>>;
