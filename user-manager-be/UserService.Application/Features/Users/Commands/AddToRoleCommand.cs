using MediatR;

namespace UserService.Application.Features.Users.Commands;

/// <summary>
/// Command to add a role to a user (upgrade role).
/// </summary>
public record AddToRoleCommand(
    Guid UserId,
    string Role
) : IRequest<Unit>;
