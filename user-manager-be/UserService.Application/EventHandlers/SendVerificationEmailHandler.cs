using MediatR;
using Microsoft.Extensions.Logging;
using UserService.Application.Common;
using UserService.Application.Configuration;
using UserService.Application.Services;
using UserService.Domain.Events;
using static UserService.Domain.Constants.EmailPurpose;

namespace UserService.Application.EventHandlers;

public sealed class SendVerificationEmailHandler
    : INotificationHandler<DomainEventNotification<UserRegisteredEvent>>
{
    private readonly IMagicLinkService _magicLinkService;
    private readonly IEmailService _emailService;
    private readonly AppSettings _appSettings;
    private readonly ILogger<SendVerificationEmailHandler> _logger;

    public SendVerificationEmailHandler(
        IMagicLinkService magicLinkService,
        IEmailService emailService,
        AppSettings appSettings,
        ILogger<SendVerificationEmailHandler> logger)
    {
        _magicLinkService = magicLinkService;
        _emailService = emailService;
        _appSettings = appSettings;
        _logger = logger;
    }

    public async Task Handle(
        DomainEventNotification<UserRegisteredEvent> notification,
        CancellationToken cancellationToken)
    {
        var evt = notification.DomainEvent;

        _logger.LogDebug("Sending verification email for UserId={UserId}, Email={Email}", evt.UserId, evt.Email);
        var (token, tokenEntity) = await _magicLinkService.GenerateMagicLinkTokenWithoutPersistingAsync(
            evt.UserId,
            MagicLinkPurpose.EmailVerification,
            evt.Email,
            cancellationToken);

        var baseUrl = _appSettings.FrontendBaseUrl.Trim() ?? "";
        var link = _magicLinkService.BuildMagicLinkUrl(token, MagicLinkPurpose.EmailVerification, baseUrl, null);
        await _emailService.SendMagicLinkEmailAsync(
            evt.Email,
            link,
            evt.Username,
            MagicLinkPurpose.EmailVerification,
            cancellationToken);
        await _magicLinkService.PersistMagicLinkTokenAsync(tokenEntity, cancellationToken);
        _logger.LogInformation("Verification email sent to {Email} for UserId={UserId}", evt.Email, evt.UserId);
    }
}
