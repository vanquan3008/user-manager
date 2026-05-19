using MediatR;
using UserService.Application.Features.Applications.Commands;
using UserService.Application.Interfaces.Services;

namespace UserService.Application.Features.Applications.Handlers;

/// <summary>
/// Handler for AddUserToApplicationCommand.
/// </summary>
public sealed class AddUserToApplicationCommandHandler : IRequestHandler<AddUserToApplicationCommand, Unit>
{
    private readonly IApplicationMembershipService _membershipService;

    public AddUserToApplicationCommandHandler(IApplicationMembershipService membershipService)
    {
        _membershipService = membershipService;
    }

    public async Task<Unit> Handle(AddUserToApplicationCommand command, CancellationToken cancellationToken)
    {
        await _membershipService.AddUserAsync(command.ApplicationId, command.UserId, command.Role, cancellationToken);
        return Unit.Value;
    }
}
