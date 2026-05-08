using MediatR;
using UserService.Application.Common;

namespace UserService.Application.Features.Roles.Commands;

/// <summary>
/// Command to delete a global role. Admin+SuperAdmin only.
/// </summary>
public record DeleteRoleCommand(Guid RoleId) : IRequest<ApplicationRoleResult>;
