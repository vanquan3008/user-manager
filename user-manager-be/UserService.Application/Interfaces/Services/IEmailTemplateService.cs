namespace UserService.Application.Interfaces.Services;
public interface IEmailTemplateService
{

    string RenderMagicLinkEmail(string userName, string magicLink, string purpose);
    string RenderWelcomeEmail(string userName);
}
