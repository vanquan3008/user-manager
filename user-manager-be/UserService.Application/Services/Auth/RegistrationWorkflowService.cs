using UserService.Application.DTOs.Auth;
using UserService.Application.Interfaces.Services;
using UserService.Domain.Entities;
using UserService.Domain.Exceptions;
using UserService.Domain.Interfaces;
using UserService.Domain.Repositories;
using UserService.Domain.ValueObjects;

namespace UserService.Application.Services.Auth;

public sealed class RegistrationWorkflowService : IRegistrationWorkflowService
{
    private readonly IIdentityService _identityService;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClock _clock;

    public RegistrationWorkflowService(
        IIdentityService identityService,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IClock clock)
    {
        _identityService = identityService;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _clock = clock;
    }

    public async Task<Guid> CreateUserAsync(
        RegisterUserRequest request,
        CancellationToken cancellationToken = default)
    {
        var identityUserId = await RegisterIdentityUserAsync(request, cancellationToken);

        try
        {
            var userProfile = BuildDomainUserProfile(identityUserId, request);

            await _userRepository.AddAsync(userProfile, cancellationToken);
            await _identityService.UpdateUserProfileIdAsync(identityUserId, userProfile.Id, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return identityUserId;
        }
        catch
        {
            await _identityService.DeleteUserAsync(identityUserId, cancellationToken);
            throw;
        }
    }

    private async Task<Guid> RegisterIdentityUserAsync(
        RegisterUserRequest request,
        CancellationToken cancellationToken)
    {
        var (succeeded, userId, errors) = await _identityService.RegisterAsync(
            request.Email,
            request.Password,
            request.Username,
            cancellationToken);

        if (!succeeded || !userId.HasValue)
            throw new RegistrationFailedException(errors);

        return userId.Value;
    }

    private User BuildDomainUserProfile(Guid userId, RegisterUserRequest request)
    {
        return User.Create(
            userId,
            Username.Create(request.Username),
            Email.Create(request.Email),
            PersonName.Create(request.FirstName, request.LastName),
            request.DateOfBirth,
            _clock.UtcNow);
    }
}
