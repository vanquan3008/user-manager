using UserService.Application.Configuration;
using UserService.Application.Interfaces.Services;
using UserService.Domain.Exceptions;
using static UserService.Domain.Constants.EmailPurpose.MagicLinkPurpose;

namespace UserService.Application.Services.Auth;

public sealed class MagicLinkDeliveryService : IMagicLinkDeliveryService
{
    private readonly IIdentityService _identityService;
    private readonly IMagicLinkService _magicLinkService;
    private readonly IEmailService _emailService;
    private readonly IApplicationAppService _applicationService;
    private readonly AppSettings _appSettings;

    public MagicLinkDeliveryService(
        IIdentityService identityService,
        IMagicLinkService magicLinkService,
        IEmailService emailService,
        IApplicationAppService applicationService,
        AppSettings appSettings)
    {
        _identityService = identityService;
        _magicLinkService = magicLinkService;
        _emailService = emailService;
        _applicationService = applicationService;
        _appSettings = appSettings;
    }

    public async Task<bool> ResendMagicLinkAsync(
        string email,
        string purpose,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        var user = await _identityService.GetUserByEmailAsync(email, cancellationToken);
        if (user == null)
            return false;

        var token = await _magicLinkService.GenerateMagicLinkAsync(
            user.Id,
            purpose,
            email,
            cancellationToken: cancellationToken);

        var baseUrl = GetMagicLinkBaseUrl(purpose);
        var magicLink = _magicLinkService.BuildMagicLinkUrl(token, purpose, baseUrl, email);

        await _emailService.SendMagicLinkEmailAsync(
            email,
            magicLink,
            user.UserName,
            purpose,
            cancellationToken);

        return true;
    }

    public async Task<bool> RequestPasswordlessLoginAsync(
        string email,
        Guid? applicationId = null,
        CancellationToken cancellationToken = default)
    {
        var user = await _identityService.GetUserByEmailAsync(email, cancellationToken);
        if (user == null)
            return true;

        var token = await _magicLinkService.GenerateMagicLinkAsync(
            user.Id,
            PasswordlessLogin,
            email,
            cancellationToken: cancellationToken);

        var clientId = await ResolveClientIdAsync(applicationId, cancellationToken);
        var magicLink = _magicLinkService.BuildMagicLinkUrl(
            token,
            PasswordlessLogin,
            GetMagicLinkBaseUrl(PasswordlessLogin),
            clientId: clientId);

        await _emailService.SendMagicLinkEmailAsync(
            email,
            magicLink,
            user.UserName,
            PasswordlessLogin,
            cancellationToken);

        return true;
    }

    private string GetMagicLinkBaseUrl(string purpose)
    {
        var url = purpose switch
        {
            PasswordReset => _appSettings.FrontendBaseUrl,
            PasswordlessLogin => _appSettings.FrontendBaseUrl,
            EmailVerification => _appSettings.FrontendBaseUrl,
            _ => _appSettings.BaseUrl
        };

        return !string.IsNullOrWhiteSpace(url)
            ? url.Trim()
            : throw new ConfigurationException(
                $"MagicLinkBaseUrl:{purpose}",
                $"Base URL is not configured for purpose '{purpose}'.");
    }

    private async Task<string?> ResolveClientIdAsync(Guid? applicationId, CancellationToken cancellationToken)
    {
        if (!applicationId.HasValue)
            return null;

        var application = await _applicationService.GetByIdAsync(applicationId.Value, cancellationToken);
        if (application == null || !application.IsActive)
            throw new ApplicationInactiveException(applicationId);

        return application.ApiKey;
    }
}
