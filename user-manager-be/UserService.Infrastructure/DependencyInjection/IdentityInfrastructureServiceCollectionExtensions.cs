using Microsoft.Extensions.DependencyInjection;
using UserService.Application.Configuration;
using UserService.Application.Interfaces.Services;
using UserService.Domain.Interfaces;
using UserService.Domain.Repositories;
using UserService.Infrastructure.Repositories;

namespace UserService.Infrastructure.DependencyInjection;

internal static class IdentityInfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddIdentityInfrastructure(this IServiceCollection services, AppSettings appSettings)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IMagicLinkTokenRepository, MagicLinkTokenRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        return services.AddSharedInfrastructureServices(appSettings);
    }
}
