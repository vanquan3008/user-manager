using Microsoft.Extensions.DependencyInjection;
using UserService.Application.Configuration;
using UserService.Infrastructure.Services;

namespace UserService.Infrastructure.DependencyInjection;

internal static class OutboxInfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddOutboxInfrastructure(this IServiceCollection services, AppSettings appSettings)
    {
        if (appSettings.Outbox.Enabled)
        {
            services.AddHostedService<OutboxProcessorService>();
        }

        return services;
    }
}
