namespace UserService.Application.Interfaces.Services;

/// <summary>
/// Email service interface for sending emails
/// </summary>
public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken = default);
    Task SendMagicLinkEmailAsync(string to, string magicLink, string userName, string purpose, CancellationToken cancellationToken = default);
    Task SendWelcomeEmailAsync(string to, string userName, CancellationToken cancellationToken = default);
    Task SendPasswordResetEmailAsync(string to, string resetLink, string userName, CancellationToken cancellationToken = default);
}
