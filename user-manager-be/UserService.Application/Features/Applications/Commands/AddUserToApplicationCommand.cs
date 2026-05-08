using MediatR;

namespace UserService.Application.Features.Applications.Commands;

/// <summary>
/// Command to add a user to an application with a specific role.
/// </summary>
public record AddUserToApplicationCommand(
    Guid ApplicationId,
    Guid UserId,
    string? Role
) : IRequest<Unit>;
