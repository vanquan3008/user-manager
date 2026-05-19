using UserService.Application.Interfaces.Services;
using UserService.Domain.Repositories;
using static UserService.Domain.Constants.EmailPurpose.MagicLinkPurpose;

namespace UserService.Application.Services.Auth;

public sealed class EmailVerificationService : IEmailVerificationService
{
    private readonly IIdentityService _identityService;
    private readonly IMagicLinkService _magicLinkService;
    private readonly IUserRepository _userRepository;
    private readonly IEmailService _emailService;
    private readonly IUnitOfWork _unitOfWork;

    public EmailVerificationService(
        IIdentityService identityService,
        IMagicLinkService magicLinkService,
        IUserRepository userRepository,
        IEmailService emailService,
        IUnitOfWork unitOfWork)
    {
        _identityService = identityService;
        _magicLinkService = magicLinkService;
        _userRepository = userRepository;
        _emailService = emailService;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> VerifyEmailAsync(
        Guid userId,
        string token,
        CancellationToken cancellationToken = default)
    {
        var result = await _identityService.VerifyEmailAsync(userId, token, cancellationToken);
        if (!result)
            return false;

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user != null)
        {
            user.VerifyEmail();
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var appUser = await _identityService.GetUserByIdAsync(userId, cancellationToken);
            if (appUser != null)
                await _emailService.SendWelcomeEmailAsync(appUser.Email, appUser.UserName, cancellationToken);
        }

        return true;
    }

    public async Task<bool> VerifyEmailWithTokenAsync(
        string token,
        CancellationToken cancellationToken = default)
    {
        var (isValid, userId) = await _magicLinkService.ValidateMagicLinkAsync(
            token,
            EmailVerification,
            cancellationToken);

        if (!isValid || !userId.HasValue)
            return false;

        var confirmed = await _identityService.ConfirmEmailAsync(userId.Value, cancellationToken);
        if (!confirmed)
            return false;

        var user = await _userRepository.GetByIdAsync(userId.Value, cancellationToken);
        if (user != null)
        {
            user.VerifyEmail();
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var appUser = await _identityService.GetUserByIdAsync(userId.Value, cancellationToken);
            if (appUser != null)
                await _emailService.SendWelcomeEmailAsync(appUser.Email, appUser.UserName, cancellationToken);
        }

        return true;
    }
}
