using Microsoft.Extensions.DependencyInjection;

using UserService.Application.Interfaces.Services;
using UserService.Application.Services;

namespace UserService.Application.DependencyInjection;

internal static class SubscriptionsModuleServiceCollectionExtensions
{
    public static IServiceCollection AddSubscriptionsModule(this IServiceCollection services)
    {
        services.AddScoped<ISubscriptionManagementService, SubscriptionManagementService>();
        return services;
    }
}
