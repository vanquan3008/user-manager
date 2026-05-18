using UserService.Application.Configuration;
using UserService.Application.Interfaces.Services;
using static UserService.Domain.Constants.EmailPurpose.MagicLinkPurpose;

namespace UserService.Application.Services.Auth;

public sealed class PasswordRecoveryService : IPasswordRecoveryService
{
    private readonly IIdentityService _identityService;
    private readonly IMagicLinkService _magicLinkService;
    private readonly IEmailService _emailService;
    private readonly AppSettings _appSettings;

    public PasswordRecoveryService(
        IIdentityService identityService,
        IMagicLinkService magicLinkService,
        IEmailService emailService,
        AppSettings appSettings)
    {
        _identityService = identityService;
        _magicLinkService = magicLinkService;
        _emailService = emailService;
        _appSettings = appSettings;
    }

    public async Task<bool> RequestPasswordResetAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        var user = await _identityService.GetUserByEmailAsync(email, cancellationToken);
        if (user == null)
            return true;

        var token = await _magicLinkService.GenerateMagicLinkAsync(
            user.Id,
            PasswordReset,
            email,
            cancellationToken: cancellationToken);

        var magicLink = _magicLinkService.BuildMagicLinkUrl(
            token,
            PasswordReset,
            GetMagicLinkBaseUrl(),
            email);

        await _emailService.SendPasswordResetEmailAsync(
            email,
            magicLink,
            user.UserName,
            cancellationToken);

        return true;
    }

    private string GetMagicLinkBaseUrl()
    {
        return !string.IsNullOrWhiteSpace(_appSettings.FrontendBaseUrl)
            ? _appSettings.FrontendBaseUrl.Trim()
            : throw new InvalidOperationException("FrontendBaseUrl is not configured for password reset.");
    }
}
