using MediatR;

namespace UserService.Application.Features.Roles.Queries;

/// <summary>
/// Query to get a global role by ID.
/// </summary>
public record GetRoleByIdQuery(Guid RoleId) : IRequest<RoleDto?>;
