using MediatR;
using UserService.Application.Features.Applications.Commands;
using UserService.Application.Interfaces.Services;

namespace UserService.Application.Features.Applications.Handlers;

public sealed class RemoveUserFromApplicationCommandHandler : IRequestHandler<RemoveUserFromApplicationCommand, Unit>
{
    private readonly IApplicationMembershipService _membershipService;

    public RemoveUserFromApplicationCommandHandler(IApplicationMembershipService membershipService)
    {
        _membershipService = membershipService;
    }

    public async Task<Unit> Handle(RemoveUserFromApplicationCommand command, CancellationToken cancellationToken)
    {
        await _membershipService.RemoveUserAsync(command.ApplicationId, command.UserId, cancellationToken);
        return Unit.Value;
    }
}
