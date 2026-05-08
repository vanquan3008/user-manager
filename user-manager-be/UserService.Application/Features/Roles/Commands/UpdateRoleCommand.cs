using MediatR;
using UserService.Application.Common;

namespace UserService.Application.Features.Roles.Commands;

/// <summary>
/// Command to update a global role. Requires ManageRoles permission.
/// </summary>
public record UpdateRoleCommand(
    Guid RoleId,
    string? Description,
    string? Permissions = null
) : IRequest<ApplicationRoleResult>;
