using MediatR;
using UserService.Application.Features.Auth.Commands;
using UserService.Application.Interfaces.Services;
using UserService.Domain.Entities;
using UserService.Domain.Exceptions;
using UserService.Domain.Interfaces;
using UserService.Domain.Repositories;
using UserService.Domain.ValueObjects;

namespace UserService.Application.Features.Auth.Handlers;

public class CreateSsoUserCommandHandler : IRequestHandler<CreateSsoUserCommand, SuccessResponse>
{
    private readonly IIdentityService _identityService;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClock _clock;

    public CreateSsoUserCommandHandler(
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

    public async Task<SuccessResponse> Handle(CreateSsoUserCommand command, CancellationToken cancellationToken)
    {
        var existing = await _identityService.GetUserByEmailAsync(command.Email, cancellationToken);
        if (existing != null)
            throw new DuplicateUserException("[Email Duplicate]", command.Email);

        var (succeeded, userId, errors) = await _identityService.RegisterAsync(
            command.Email,
            command.Password,
            command.Username,
            cancellationToken);
        if (!succeeded || !userId.HasValue)
        {
            throw new BusinessRuleValidationException(
                "[Not Create SSO] - ",
                $"[SSO user creation failed] : {string.Join(", ", errors)}");
        }

        try
        {
            var user = User.Create(
                userId.Value,
                Username.Create(command.Username),
                Email.Create(command.Email),
                PersonName.Create(command.FirstName, command.LastName),
                ResolveDateOfBirth(command.DateOfBirth),
                _clock.UtcNow);
            user.EnableSSO();

            await _userRepository.AddAsync(user, cancellationToken);
            await _identityService.UpdateUserProfileIdAsync(userId.Value, user.Id, cancellationToken);
            await _identityService.AddToRoleAsync(userId.Value, Domain.Constants.RoleNames.Guest, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            await _identityService.DeleteUserAsync(userId.Value, cancellationToken);
            throw;
        }

        return new SuccessResponse(true, $"[SSO Account created] '{command.Email}'.");
    }

    private static DateOnly ResolveDateOfBirth(DateOnly? dateOfBirth)
    {
        if (dateOfBirth.HasValue &&
            dateOfBirth.Value != default &&
            dateOfBirth.Value.Year >= 1900)
        {
            return dateOfBirth.Value;
        }

        return new DateOnly(1990, 1, 1);
    }
}
