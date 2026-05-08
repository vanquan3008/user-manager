using MediatR;
using UserService.Application.Common;

namespace UserService.Application.Features.Roles.Commands;

/// <summary>
/// Command to create a global role. Requires ManageRoles permission.
/// </summary>
public record CreateRoleCommand(
    string Name,
    string? Description,
    string? Permissions = null
) : IRequest<ApplicationRoleResult>;
