using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MediatR;
using FluentValidation;
using UserService.Application.Behaviors;
using UserService.Application.Features.OAuth.Policies;
using UserService.Application.Interfaces.Services;
using UserService.Application.Services;

namespace UserService.Application.DependencyInjection;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ApplicationServiceCollectionExtensions).Assembly));
        services.AddValidatorsFromAssembly(typeof(ApplicationServiceCollectionExtensions).Assembly);
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddScoped<IOAuthRedirectUriPolicy, OAuthRedirectUriPolicy>();
        
        return services;
    }
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        services.AddApplicationServices();
        services.AddScoped<IAuthServices, AuthService>();
        services.AddScoped<IApplicationAppService, ApplicationAppService>();
        services.AddScoped<IUserApplicationAppService, UserApplicationAppService>();
        services.AddScoped<IApplicationOAuthScopeAppService, ApplicationOAuthScopeAppService>();
        services.AddScoped<IClaimsFactory, ClaimsFactory>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IMagicLinkService, MagicLinkService>();
        services.AddScoped<IApplicationCacheService, ApplicationCacheService>();
        services.AddScoped<IUserPermissionService, UserPermissionService>();
        services.AddScoped<IPlanApplicationRoleValidator, PlanApplicationRoleValidator>();
        return services;
    }
}
