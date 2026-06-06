using MediatR;
using UserService.Application.Features.Auth.Commands;
using UserService.Domain.Exceptions;
using UserService.Domain.Interfaces;
using UserService.Domain.Repositories;

namespace UserService.Application.Features.Auth.Handlers;

public class RevokeSsoAppAccessCommandHandler : IRequestHandler<RevokeSsoAppAccessCommand, SuccessResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IApplicationMembershipRepository _membershipRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RevokeSsoAppAccessCommandHandler(
        IUserRepository userRepository,
        IApplicationMembershipRepository membershipRepository,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _membershipRepository = membershipRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<SuccessResponse> Handle(RevokeSsoAppAccessCommand command, CancellationToken cancellationToken)
    {
        var domainUser = await _userRepository.GetByIdAsync(command.SsoUserId, cancellationToken);
        if (domainUser == null)
            throw new BusinessRuleValidationException("UserNotFound", "User not found.");
        if (!domainUser.IsSSOAccount)
            throw new BusinessRuleValidationException("SSOOnlyAccess", "User is not an SSO account.");

        var membership = await _membershipRepository.GetByUserAndAppAsync(
            command.SsoUserId,
            command.ApplicationId,
            cancellationToken);
        if (membership == null || !membership.IsActive)
        {
            throw new BusinessRuleValidationException(
                "SSOAccessNotFound",
                "SSO user does not have active access to this application.");
        }

        membership.Remove();
        await _membershipRepository.UpdateAsync(membership, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new SuccessResponse(true, "SSO user app access revoked.");
    }
}
