using Microsoft.Extensions.DependencyInjection;
using UserService.Application.Configuration;
using UserService.Application.Interfaces.Services;
using UserService.Domain.Interfaces;
using UserService.Domain.Repositories;
using UserService.Infrastructure.Repositories;
using UserService.Application.Interfaces.Services.Security;
using UserService.Infrastructure.Security;

namespace UserService.Infrastructure.DependencyInjection;

internal static class IdentityInfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddIdentityInfrastructure(this IServiceCollection services, AppSettings appSettings)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IMagicLinkTokenRepository, MagicLinkTokenRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IUserSessionRepository, UserSessionRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddSingleton<ISigningKeyProvider, SigningKeyProvider>();
        services.AddSingleton<IMfaChallengeService, MfaChallengeService>();
        services.AddScoped<IMfaService, MfaService>();
        if (appSettings.Security.MaintenanceEnabled)
            services.AddHostedService<UserService.Infrastructure.Services.SecurityMaintenanceService>();
        return services.AddSharedInfrastructureServices(appSettings);
    }
}
