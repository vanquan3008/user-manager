using MediatR;
using UserService.Application.Features.Auth.Commands;
using UserService.Application.Services;
using UserService.Domain.Exceptions;
using UserService.Domain.Repositories;

namespace UserService.Application.Features.Auth.Handlers;

public class RevokeSSOAppAccessCommandHandler : IRequestHandler<RevokeSSOAppAccessCommand, SuccessResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IUserApplicationRepository _userApplicationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RevokeSSOAppAccessCommandHandler(
        IUserRepository userRepository,
        IUserApplicationRepository userApplicationRepository,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _userApplicationRepository = userApplicationRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<SuccessResponse> Handle(RevokeSSOAppAccessCommand command, CancellationToken cancellationToken)
    {
        var domainUser = await _userRepository.GetByIdAsync(command.SSOUserId, cancellationToken);
        if (domainUser == null)
            throw new BusinessRuleValidationException("UserNotFound", "User not found.");
        if (!domainUser.IsSSOAccount)
            throw new BusinessRuleValidationException("SsoOnlyAccess", "User is not an SSO account.");

        var membership = await _userApplicationRepository.GetByUserAndAppAsync(command.SSOUserId, command.ApplicationId, cancellationToken);
        if (membership == null || !membership.IsActive)
            throw new BusinessRuleValidationException("SsoAccessNotFound", "SSO user does not have active access to this application.");

        membership.Deactivate();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new SuccessResponse(true, "SSO user app access revoked.");
    }
}
