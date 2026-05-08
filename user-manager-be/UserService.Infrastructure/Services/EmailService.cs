using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using Serilog;
using UserService.Application.Configuration;
using UserService.Application.Interfaces.Services;
using UserService.Domain.Constants;

namespace UserService.Infrastructure.Services;
public class EmailService : IEmailService
{
    private readonly EmailSettings _emailSettings;
    private readonly IEmailTemplateService _templateService;

    public EmailService(
        IOptions<EmailSettings> emailSettings,
        IEmailTemplateService templateService)
    {
        _emailSettings = emailSettings.Value;
        _templateService = templateService;
    }

    public async Task SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken = default)
    {
        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_emailSettings.Sender.FromName ?? string.Empty, _emailSettings.Sender.FromEmail));
            message.To.Add(MailboxAddress.Parse(to));
            message.Subject = subject ?? string.Empty;

            if (!string.IsNullOrWhiteSpace(_emailSettings.Sender.ReplyToEmail))
            {
                message.ReplyTo.Add(MailboxAddress.Parse(_emailSettings.Sender.ReplyToEmail));
            }

            var builder = new BodyBuilder { HtmlBody = body ?? string.Empty };
            message.Body = builder.ToMessageBody();

            using var client = new SmtpClient();
            client.Timeout = _emailSettings.Smtp.Timeout;

            var secureOption = _emailSettings.Smtp.EnableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto;
            await client.ConnectAsync(_emailSettings.Smtp.Host, _emailSettings.Smtp.Port, secureOption, cancellationToken);

            if (!string.IsNullOrWhiteSpace(_emailSettings.Smtp.Username))
            {
                await client.AuthenticateAsync(_emailSettings.Smtp.Username, _emailSettings.Smtp.Password, cancellationToken);
            }

            await client.SendAsync(message, cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);

            Log.Information("Email sent successfully to {Email} with subject: {Subject}", to, subject);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to send email to {Email} with subject: {Subject}", to, subject);
            throw;
        }
    }

    public async Task SendMagicLinkEmailAsync(string to, string magicLink, string userName, string purpose, CancellationToken cancellationToken = default)
    {
        var app = _emailSettings.ResolveApplicationDisplayName();
        var subject = purpose switch
        {
            EmailPurpose.MagicLinkPurpose.PasswordlessLogin => $"Sign in to {app}",
            EmailPurpose.MagicLinkPurpose.EmailVerification => $"Verify your email — {app}",
            EmailPurpose.MagicLinkPurpose.PasswordReset => $"Reset your password — {app}",
            _ => $"Action required — {app}"
        };

        var body = _templateService.RenderMagicLinkEmail(userName, magicLink, purpose);
        
        await SendEmailAsync(to, subject, body, cancellationToken);
    }

    public async Task SendWelcomeEmailAsync(string to, string userName, CancellationToken cancellationToken = default)
    {
        var app = _emailSettings.ResolveApplicationDisplayName();
        var subject = $"Welcome to {app}!";
        var body = _templateService.RenderWelcomeEmail(userName);
        
        await SendEmailAsync(to, subject, body, cancellationToken);
    }

    public async Task SendPasswordResetEmailAsync(string to, string resetLink, string userName, CancellationToken cancellationToken = default)
    {
        await SendMagicLinkEmailAsync(to, resetLink, userName, EmailPurpose.MagicLinkPurpose.PasswordReset, cancellationToken);
    }
}
