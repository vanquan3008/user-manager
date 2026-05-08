using MediatR;
using UserService.Application.Features.Applications.Commands;
using UserService.Application.Services;
using UserService.Domain.Entities;
using UserService.Domain.Exceptions;
using UserService.Domain.Repositories;

namespace UserService.Application.Features.Applications.Handlers;

/// <summary>
/// Handler for AddUserToApplicationCommand.
/// </summary>
public sealed class AddUserToApplicationCommandHandler : IRequestHandler<AddUserToApplicationCommand, Unit>
{
    private readonly IApplicationRepository _applications;
    private readonly IUserApplicationRepository _userApplications;
    private readonly IApplicationRoleService _roleService;
    private readonly IUnitOfWork _unitOfWork;

    public AddUserToApplicationCommandHandler(
        IApplicationRepository applications,
        IUserApplicationRepository userApplications,
        IApplicationRoleService roleService,
        IUnitOfWork unitOfWork)
    {
        _applications = applications;
        _userApplications = userApplications;
        _roleService = roleService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(AddUserToApplicationCommand command, CancellationToken cancellationToken)
    {
        var app = await _applications.GetByIdAsync(command.ApplicationId, cancellationToken);
        if (app == null)
            throw new ApplicationNotFoundException(command.ApplicationId);

        var existing = await _userApplications.GetByUserAndAppAsync(command.UserId, command.ApplicationId, cancellationToken);
        if (existing != null)
            throw new UserAlreadyInApplicationException(command.UserId, command.ApplicationId);

        var roleName = string.IsNullOrWhiteSpace(command.Role) ? "Guest" : command.Role.Trim();
        var roleExists = await _roleService.RoleExistsForApplicationAsync(command.ApplicationId, roleName, cancellationToken);
        if (!roleExists)
            throw new RoleNotFoundException(roleName, command.ApplicationId);

        var userApp = UserApplication.Create(
            Guid.NewGuid(),
            command.UserId,
            command.ApplicationId,
            roleName);

        await _userApplications.AddAsync(userApp, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
