using MediatR;
using UserService.Application.Common;

namespace UserService.Application.Features.ApplicationRoles.Commands;

public record CreateApplicationRoleCommand(
    Guid ApplicationId,
    string Name,
    string? Description
) : IRequest<ApplicationRoleResult>;
