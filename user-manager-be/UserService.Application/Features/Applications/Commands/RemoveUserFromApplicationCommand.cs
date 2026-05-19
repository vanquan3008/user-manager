using MediatR;

namespace UserService.Application.Features.Applications.Commands;

/// <summary>
/// Command to remove a user from an application.
/// </summary>
public record RemoveUserFromApplicationCommand(
    Guid ApplicationId,
    Guid UserId
) : IRequest<Unit>;
