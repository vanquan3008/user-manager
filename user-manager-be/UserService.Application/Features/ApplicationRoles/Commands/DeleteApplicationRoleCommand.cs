using MediatR;
using UserService.Application.Common;

namespace UserService.Application.Features.ApplicationRoles.Commands;
public record DeleteApplicationRoleCommand(
    Guid ApplicationId,
    string RoleName
) : IRequest<ApplicationRoleResult>;
