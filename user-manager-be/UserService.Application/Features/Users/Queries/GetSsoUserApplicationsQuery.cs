using MediatR;
using UserService.Application.DTOs.Users;

namespace UserService.Application.Features.Users.Queries;

public sealed record GetSSOUserApplicationsQuery(Guid UserId)
    : IRequest<IReadOnlyList<SSOUserApplicationDto>>;
