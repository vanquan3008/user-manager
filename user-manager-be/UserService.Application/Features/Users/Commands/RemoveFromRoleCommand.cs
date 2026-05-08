using MediatR;

namespace UserService.Application.Features.Users.Commands;

/// <summary>
/// Command to remove a role from a user.
/// </summary>
public record RemoveFromRoleCommand(
    Guid UserId,
    string Role
) : IRequest<Unit>;
