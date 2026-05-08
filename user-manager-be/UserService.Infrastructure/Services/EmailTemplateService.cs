using Microsoft.Extensions.Options;
using UserService.Application.Configuration;
using UserService.Application.Interfaces.Services;
using UserService.Application.Services;
using UserService.Domain.Constants;

namespace UserService.Infrastructure.Services;
public class EmailTemplateService : IEmailTemplateService
{
    private readonly EmailSettings _emailSettings;
    private readonly string _templatesPath;
    private readonly Dictionary<string, string> _templateCache = new();

    public EmailTemplateService(IOptions<EmailSettings> emailSettings)
    {
        _emailSettings = emailSettings.Value;
        _templatesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _emailSettings.Templates.TemplatesPath);
    }

    public string RenderMagicLinkEmail(string userName, string magicLink, string purpose)
    {
        var app = _emailSettings.ResolveApplicationDisplayName();
        var (subject, actionText, actionDescription) = purpose switch
        {
            EmailPurpose.MagicLinkPurpose.PasswordlessLogin => ($"Sign in to {app}", "Sign in", "sign in to your account"),
            EmailPurpose.MagicLinkPurpose.EmailVerification => ($"Verify your email — {app}", "Verify email", "verify your email address"),
            EmailPurpose.MagicLinkPurpose.PasswordReset => ($"Reset your password — {app}", "Reset password", "reset your password"),
            _ => ($"Action required — {app}", "Continue", "complete your action")
        };

        var template = GetTemplate("MagicLinkEmail.html");
        
        return template
            .Replace("{{UserName}}", userName)
            .Replace("{{MagicLink}}", magicLink)
            .Replace("{{Subject}}", subject)
            .Replace("{{ActionText}}", actionText)
            .Replace("{{ActionDescription}}", actionDescription)
            .Replace("{{ApplicationName}}", app)
            .Replace("{{PrimaryColor}}", _emailSettings.Templates.PrimaryColor)
            .Replace("{{BaseUrl}}", _emailSettings.Templates.BaseUrl);
    }

    public string RenderWelcomeEmail(string userName)
    {
        var app = _emailSettings.ResolveApplicationDisplayName();
        var template = GetTemplate("WelcomeEmail.html");
        
        return template
            .Replace("{{UserName}}", userName)
            .Replace("{{ApplicationName}}", app)
            .Replace("{{PrimaryColor}}", _emailSettings.Templates.PrimaryColor)
            .Replace("{{BaseUrl}}", _emailSettings.Templates.BaseUrl);
    }

    private string GetTemplate(string templateName)
    {
        if (_templateCache.TryGetValue(templateName, out var cachedTemplate))
        {
            return cachedTemplate;
        }
        var templatePath = Path.Combine(_templatesPath, templateName);
        if (!File.Exists(templatePath))
        {
            throw new FileNotFoundException($"Email template not found: {templatePath}");
        }

        var template = File.ReadAllText(templatePath);
        
        _templateCache[templateName] = template;
        
        return template;
    }
}
