namespace UserService.Application.Configuration;

public class EmailSettings
{
    public const string SectionName = "EmailSettings";
    public SmtpSettings Smtp { get; set; } = new();
    public SenderSettings Sender { get; set; } = new();
    public TemplateSettings Templates { get; set; } = new();

    public string ApplicationDisplayName { get; set; } = string.Empty;

    public string ResolveApplicationDisplayName()
    {
        if (!string.IsNullOrWhiteSpace(ApplicationDisplayName))
            return ApplicationDisplayName.Trim();

        if (!string.IsNullOrWhiteSpace(Sender.FromName))
            return Sender.FromName.Trim();

        return "Application";
    }
}

public class SmtpSettings
{
    public string Host { get; set; } = "smtp.gmail.com";
    public int Port { get; set; } = 587;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool EnableSsl { get; set; } = true;
    public int Timeout { get; set; } = 30000;
}

public class SenderSettings
{
    public string FromEmail { get; set; } = "noreply@musicapp.com";
    public string FromName { get; set; } = "Music App";
    public string ReplyToEmail { get; set; } = "support@musicapp.com";
}

public class TemplateSettings
{
    public string TemplatesPath { get; set; } = "EmailTemplates";
    public string BaseUrl { get; set; } = "https://musicapp.com";
    public string LogoUrl { get; set; } = "https://musicapp.com/logo.png";
    public string PrimaryColor { get; set; } = "#1DB954";
}
