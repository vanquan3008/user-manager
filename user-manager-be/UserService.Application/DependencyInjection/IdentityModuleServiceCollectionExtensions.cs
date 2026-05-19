using Microsoft.Extensions.DependencyInjection;
using UserService.Application.Interfaces.Services;
using UserService.Application.Services;
using UserService.Application.Services.Auth;

namespace UserService.Application.DependencyInjection;

internal static class IdentityModuleServiceCollectionExtensions
{
    public static IServiceCollection AddIdentityModule(this IServiceCollection services)
    {
        services.AddScoped<AuthResponseFactory>();
        services.AddScoped<IRegistrationService, RegistrationService>();
        services.AddScoped<IRegistrationWorkflowService, RegistrationWorkflowService>();
        services.AddScoped<ILoginService, LoginService>();
        services.AddScoped<ISSOAuthenticationService, SSOAuthenticationService>();
        services.AddScoped<IMagicLinkAuthService, MagicLinkAuthService>();
        services.AddScoped<IMagicLinkDeliveryService, MagicLinkDeliveryService>();
        services.AddScoped<IPasswordRecoveryService, PasswordRecoveryService>();
        services.AddScoped<IEmailVerificationService, EmailVerificationService>();
        services.AddScoped<IClaimsFactory, ClaimsFactory>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IMagicLinkService, MagicLinkService>();
        return services;
    }
}
