using Microsoft.Extensions.DependencyInjection;
using UserService.Domain.Repositories;
using UserService.Infrastructure.Repositories;

namespace UserService.Infrastructure.DependencyInjection;

internal static class SubscriptionsInfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddSubscriptionsInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
        services.AddScoped<ISubscriptionPlanRepository, SubscriptionPlanRepository>();
        return services;
    }
}
