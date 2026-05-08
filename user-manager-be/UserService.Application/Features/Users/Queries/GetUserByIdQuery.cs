using MediatR;

namespace UserService.Application.Features.Users.Queries;

/// <summary>
/// Query to get a specific user by ID with their roles.
/// </summary>
public record GetUserByIdQuery(
    Guid UserId
) : IRequest<UserWithRolesDto?>;
