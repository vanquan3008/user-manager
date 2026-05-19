using Microsoft.Extensions.DependencyInjection;
using UserService.Application.Interfaces.Services;
using UserService.Application.Services;

namespace UserService.Application.DependencyInjection;

internal static class AuthorizationModuleServiceCollectionExtensions
{
    public static IServiceCollection AddAuthorizationModule(this IServiceCollection services)
    {
        services.AddScoped<IApplicationAccessService, ApplicationAccessService>();
        services.AddScoped<IUserPermissionService, UserPermissionService>();
        services.AddScoped<ICapabilityService, CapabilityService>();
        services.AddScoped<IOAuthFlowService, OAuthFlowService>();
        return services;
    }
}
