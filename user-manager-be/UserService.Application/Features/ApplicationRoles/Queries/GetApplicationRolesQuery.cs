using MediatR;

namespace UserService.Application.Features.ApplicationRoles.Queries;

public record GetApplicationRolesQuery(Guid ApplicationId) : IRequest<IReadOnlyList<ApplicationRoleDto>>;
