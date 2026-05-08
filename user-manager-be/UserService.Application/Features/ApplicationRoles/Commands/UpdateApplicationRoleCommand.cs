using MediatR;
using UserService.Application.Common;

namespace UserService.Application.Features.ApplicationRoles.Commands;

/// <summary>
/// Command to update a role's description for an application.
/// </summary>
public record UpdateApplicationRoleCommand(
    Guid ApplicationId,
    string RoleName,
    string? Description
) : IRequest<ApplicationRoleResult>;
