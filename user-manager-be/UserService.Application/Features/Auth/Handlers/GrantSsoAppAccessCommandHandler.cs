using MediatR;
using UserService.Application.Features.Auth.Commands;
using UserService.Application.Interfaces.Services;
using UserService.Domain.Entities;
using UserService.Domain.Exceptions;
using UserService.Domain.Interfaces;
using UserService.Domain.Repositories;

namespace UserService.Application.Features.Auth.Handlers;

public class GrantSSOAppAccessCommandHandler : IRequestHandler<GrantSSOAppAccessCommand, SuccessResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IApplicationRepository _applicationRepository;
    private readonly IApplicationMembershipRepository _membershipRepository;
    private readonly IApplicationRoleService _applicationRoleService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClock _clock;

    public GrantSSOAppAccessCommandHandler(
        IUserRepository userRepository,
        IApplicationRepository applicationRepository,
        IApplicationMembershipRepository membershipRepository,
        IApplicationRoleService applicationRoleService,
        IUnitOfWork unitOfWork,
        IClock clock)
    {
        _userRepository = userRepository;
        _applicationRepository = applicationRepository;
        _membershipRepository = membershipRepository;
        _applicationRoleService = applicationRoleService;
        _unitOfWork = unitOfWork;
        _clock = clock;
    }

    public async Task<SuccessResponse> Handle(GrantSSOAppAccessCommand command, CancellationToken cancellationToken)
    {
        var domainUser = await _userRepository.GetByIdAsync(command.SSOUserId, cancellationToken);
        if (domainUser == null)
            throw new UserNotFoundException(command.SSOUserId);
        if (!domainUser.IsSSOAccount)
            throw new BusinessRuleValidationException("SSOOnlyAccess", "User is not an SSO account.");

        var app = await _applicationRepository.GetByIdAsync(command.ApplicationId, cancellationToken);
        if (app == null || !app.IsActive)
            throw new ApplicationInactiveException(command.ApplicationId);

        var roleId = await _applicationRoleService.GetRoleIdForUserAssignmentAsync(
            command.ApplicationId,
            command.Role,
            cancellationToken);
        if (!roleId.HasValue)
            throw new RoleNotFoundException(command.Role, command.ApplicationId);

        var existing = await _membershipRepository.GetByUserAndAppAsync(
            command.SSOUserId,
            command.ApplicationId,
            cancellationToken);
        if (existing != null)
        {
            if (!existing.IsActive)
            {
                existing.Activate(_clock.UtcNow);
                existing.ChangeRole(roleId.Value);
                await _membershipRepository.UpdateAsync(existing, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                return new SuccessResponse(true, "SSO user app access reactivated.");
            }

            throw new BusinessRuleValidationException(
                "DuplicateSSOApplicationAccess",
                "SSO user already has access to this application.");
        }

        var membership = ApplicationMembership.Join(
            Guid.NewGuid(),
            command.SSOUserId,
            command.ApplicationId,
            roleId.Value,
            _clock.UtcNow);

        await _membershipRepository.AddAsync(membership, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new SuccessResponse(
            true,
            $"SSO user granted access to application '{app.Name}' with role '{command.Role}'.");
    }
}
