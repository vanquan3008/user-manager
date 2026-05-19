using Microsoft.Extensions.DependencyInjection;
using UserService.Application.Interfaces.Services;
using UserService.Application.Services;

namespace UserService.Application.DependencyInjection;

internal static class ApplicationsModuleServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationsModule(this IServiceCollection services)
    {
        services.AddScoped<IApplicationAppService, ApplicationAppService>();
        services.AddScoped<IApplicationMembershipService, ApplicationMembershipService>();
        services.AddScoped<IApplicationOAuthScopeAppService, ApplicationOAuthScopeAppService>();
        services.AddScoped<IApplicationCacheService, ApplicationCacheService>();
        return services;
    }
}
