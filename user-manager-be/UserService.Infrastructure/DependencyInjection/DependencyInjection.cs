using Microsoft.Extensions.DependencyInjection;
using UserService.Application.Configuration;
using UserService.Application.Interfaces.Services;
using CloudinaryDotNet;
using UserService.Domain.Interfaces;
using UserService.Domain.Repositories;
using UserService.Infrastructure.Repositories;
using UserService.Infrastructure.Services;

namespace UserService.Infrastructure.DependencyInjection;

public static class DependencyInjectionExtension
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        AppSettings appSettings)
    {
        return services
            .AddIdentityInfrastructure(appSettings)
            .AddApplicationsInfrastructure()
            .AddAuthorizationInfrastructure()
            .AddSubscriptionsInfrastructure()
            .AddOutboxInfrastructure(appSettings);
    }

    internal static IServiceCollection AddSharedInfrastructureServices(
        this IServiceCollection services, 
        AppSettings appSettings)
    {
        // Infrastructure-owned service implementations
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<IEmailTemplateService, EmailTemplateService>();
        services.AddScoped<IEmailService, EmailService>();

        // File storage and related
        services.AddFileStorage(appSettings);

        services.AddHttpClient<IThirdPartyApiService, ThirdPartyApiService>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
        });
        services.AddScoped<IBackchannelLogoutService, BackchannelLogoutService>();

        services.AddSingleton<IClock, SystemClock>();
        services.AddSingleton<IHashingService, HashingService>();
        return services;
    }

    private static IServiceCollection AddFileStorage(
        this IServiceCollection services, 
        AppSettings appSettings)
    {
        var cloudName = appSettings.Cloudinary.CloudName;
        var apiKey = appSettings.Cloudinary.ApiKey;
        var apiSecret = appSettings.Cloudinary.ApiSecret;

        if (string.IsNullOrWhiteSpace(cloudName) ||
            string.IsNullOrWhiteSpace(apiKey) ||
            string.IsNullOrWhiteSpace(apiSecret))
        {
            throw new InvalidOperationException(
                "Cloudinary configuration is required. Please set AppSettings:Cloudinary:CloudName, ApiKey, ApiSecret.");
        }

        services.AddSingleton(appSettings.Cloudinary);
        services.AddSingleton(_ =>
        {
            var account = new Account(cloudName, apiKey, apiSecret);
            return new Cloudinary(account) { Api = { Secure = true } };
        });
        services.AddScoped<IFileStorageService, CloudinaryFileStorageService>();
        return services;
    }

}
