namespace UserService.Application.Exceptions;

public sealed class SSOLoginRequiredException : UnauthorizedAccessException
{
    public SSOLoginRequiredException(
        string email,
        Guid? applicationId = null,
        string? returnUrl = null)
        : base("SSO account must use the SSO login page.")
    {
        Email = email;
        ApplicationId = applicationId;
        ReturnUrl = returnUrl;
    }

    public string Email { get; }
    public Guid? ApplicationId { get; }
    public string? ReturnUrl { get; }
}
