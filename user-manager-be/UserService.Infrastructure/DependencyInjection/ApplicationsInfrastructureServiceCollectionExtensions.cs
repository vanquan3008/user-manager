using Microsoft.Extensions.DependencyInjection;
using UserService.Domain.Repositories;
using UserService.Infrastructure.Repositories;

namespace UserService.Infrastructure.DependencyInjection;

internal static class ApplicationsInfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationsInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IApplicationRepository, ApplicationRepository>();
        services.AddScoped<IApplicationMembershipRepository, ApplicationMembershipRepository>();
        services.AddScoped<IApplicationRoleRepository, ApplicationRoleRepository>();
        services.AddScoped<IApplicationSubscriptionRepository, ApplicationSubscriptionRepository>();
        services.AddScoped<IApplicationOAuthScopeRepository, ApplicationOAuthScopeRepository>();
        return services;
    }
}
