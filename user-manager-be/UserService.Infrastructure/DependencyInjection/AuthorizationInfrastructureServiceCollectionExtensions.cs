using Microsoft.Extensions.DependencyInjection;
using UserService.Application.Interfaces.Services;
using UserService.Domain.Repositories;
using UserService.Infrastructure.Repositories;
using UserService.Infrastructure.Services;

namespace UserService.Infrastructure.DependencyInjection;

internal static class AuthorizationInfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddAuthorizationInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IPermissionRepository, PermissionRepository>();
        services.AddScoped<IRolePermissionRepository, RolePermissionRepository>();
        services.AddScoped<IUserOAuthConsentRepository, UserOAuthConsentRepository>();
        services.AddScoped<IOAuthAuthorizationCodeRepository, OAuthAuthorizationCodeRepository>();
        services.AddScoped<IOAuthRefreshTokenRepository, OAuthRefreshTokenRepository>();
        services.AddScoped<IApplicationRoleService, ApplicationRoleService>();
        services.AddScoped<ISystemRoleService, SystemRoleService>();
        return services;
    }
}
