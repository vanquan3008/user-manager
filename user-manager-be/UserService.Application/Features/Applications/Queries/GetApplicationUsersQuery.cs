using MediatR;
using UserService.Application.DTOs.Applications;

namespace UserService.Application.Features.Applications.Queries;

public sealed record GetApplicationUsersQuery(Guid ApplicationId) : IRequest<IReadOnlyList<ApplicationUserDto>>;

