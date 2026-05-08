using MediatR;
using UserService.Domain.Interfaces;
using UserService.Application.Features.Auth.Commands;
using UserService.Application.Services;
using UserService.Domain.Entities;
using UserService.Domain.Exceptions;
using UserService.Domain.Repositories;

namespace UserService.Application.Features.Auth.Handlers;

public class GrantSSOAppAccessCommandHandler : IRequestHandler<GrantSSOAppAccessCommand, SuccessResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IApplicationRepository _applicationRepository;
    private readonly IUserApplicationRepository _userApplicationRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClock _clock;

    public GrantSSOAppAccessCommandHandler(
        IUserRepository userRepository,
        IApplicationRepository applicationRepository,
        IUserApplicationRepository userApplicationRepository,
        IUnitOfWork unitOfWork,
        IClock clock)
    {
        _userRepository = userRepository;
        _applicationRepository = applicationRepository;
        _userApplicationRepository = userApplicationRepository;
        _unitOfWork = unitOfWork;
        _clock = clock;
    }

    public async Task<SuccessResponse> Handle(GrantSSOAppAccessCommand command, CancellationToken cancellationToken)
    {
        var domainUser = await _userRepository.GetByIdAsync(command.SSOUserId, cancellationToken);
        if (domainUser == null)
            throw new UserNotFoundException(command.SSOUserId);
        if (!domainUser.IsSSOAccount)
            throw new BusinessRuleValidationException("SsoOnlyAccess", "User is not an SSO account.");
        var app = await _applicationRepository.GetByIdAsync(command.ApplicationId, cancellationToken);
        if (app == null)
            throw new ApplicationNotFoundException(command.ApplicationId);
        if (!app.IsActive)
            throw new BusinessRuleValidationException("ApplicationActive", "Application is inactive.");

        var existing = await _userApplicationRepository.GetByUserAndAppAsync(command.SSOUserId, command.ApplicationId, cancellationToken);
        if (existing != null)
        {
            if (!existing.IsActive)
            {
                existing.Activate(_clock.UtcNow);
                existing.UpdateRole(command.Role);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                return new SuccessResponse(true, "SSO user app access reactivated.");
            }
            throw new BusinessRuleValidationException(
                "DuplicateSsoApplicationAccess",
                "SSO user already has access to this application.");
        }

        var userApp = UserApplication.Create(Guid.NewGuid(), command.SSOUserId, command.ApplicationId, command.Role);
        await _userApplicationRepository.AddAsync(userApp, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new SuccessResponse(true, $"SSO user granted access to application '{app.Name}' with role '{command.Role}'.");
    }
}
