using UserService.Api.Filters;
using UserService.Application.Configuration;
using UserService.Application.DTOs.Common;
using Microsoft.AspNetCore.Mvc;
using UserService.Api.Authorization.Services;
using FluentValidation;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using UserService.Api.Security;
using UserService.Application.Interfaces.Services.Security;
using UserService.Infrastructure.Data;

namespace UserService.Api.Extensions;

public static class ApiServiceExtensions
{
    public static IServiceCollection AddApiServices(
        this IServiceCollection services,
        IConfiguration configuration,
        AppSettings appSettings)
    {
        services.AddSingleton(appSettings);
        services.Configure<EmailSettings>(configuration.GetSection(EmailSettings.SectionName));
        services.AddValidatorsFromAssembly(typeof(UserService.Api.Extensions.ApiServiceExtensions).Assembly);
        services.AddScoped<IAuthorizationExchangeService, AuthorizationExchangeService>();
        services.AddHttpContextAccessor();
        services.AddMemoryCache();
        services.AddScoped<IRequestContext, HttpRequestContext>();
        services.AddPlatformDataProtection(appSettings);
        services.AddPlatformRateLimiting(appSettings);
        services.AddTrustedForwardedHeaders(appSettings);
        services.AddHealthChecks()
            .AddDbContextCheck<UserDbContext>("database", tags: new[] { "ready" });

        services.AddDatabase(configuration, appSettings);
        services.AddCustomIdentity();
        services.AddOpenIddictServer(appSettings);

        services.AddCorsPolicy(appSettings);
        services.AddJwtAuthentication(appSettings);
        services.AddCustomAuthorization();
        services.AddScoped<ApplicationScopeActionFilter>();
        services.AddControllers(options =>
        {
            options.Filters.AddService<UserService.Api.Filters.ApplicationScopeActionFilter>();
        }).ConfigureApiBehaviorOptions(o =>
        {
            o.InvalidModelStateResponseFactory = context =>
            {
                var errors = context.ModelState
                    .Where(kvp => kvp.Value is { Errors.Count: > 0 })
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray());

                var response = new ApiResponse<object>(
                    false,
                    null,
                    "Validation Failed",
                    new ErrorDetail("MODEL_VALIDATION_ERROR", "Validation Failed", errors));

                return new BadRequestObjectResult(response);
            };
        });
        services.AddRouting(options => options.LowercaseUrls = true);
        services.AddEndpointsApiExplorer();
        services.AddSwaggerDocumentation();

        return services;
    }

    /// <summary>
    /// Data Protection keys (used for signing-key encryption and MFA challenges) are persisted in the
    /// database so every instance and restart shares them; optionally encrypted with a certificate.
    /// </summary>
    private static IServiceCollection AddPlatformDataProtection(this IServiceCollection services, AppSettings appSettings)
    {
        var builder = services.AddDataProtection()
            .SetApplicationName("UserService")
            .PersistKeysToDbContext<UserDbContext>();

        var certPath = appSettings.Security.DataProtectionCertificatePath;
        if (!string.IsNullOrWhiteSpace(certPath))
        {
            builder.ProtectKeysWithCertificate(
                X509CertificateLoader.LoadPkcs12FromFile(certPath, appSettings.Security.DataProtectionCertificatePassword));
        }
        else if (appSettings.IsProduction())
        {
            Serilog.Log.Warning("Data Protection keys are stored unencrypted. Configure AppSettings:Security:DataProtectionCertificatePath in production.");
        }

        return services;
    }

    /// <summary>X-Forwarded-* headers are only trusted from configured proxies/networks (loopback by default).</summary>
    private static IServiceCollection AddTrustedForwardedHeaders(this IServiceCollection services, AppSettings appSettings)
    {
        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost;
            options.ForwardLimit = 2;
            foreach (var proxy in appSettings.Security.ForwardedHeaders.KnownProxies)
            {
                if (IPAddress.TryParse(proxy, out var ip))
                    options.KnownProxies.Add(ip);
            }
            foreach (var network in appSettings.Security.ForwardedHeaders.KnownNetworks)
            {
                if (System.Net.IPNetwork.TryParse(network, out var parsed))
                    options.KnownIPNetworks.Add(parsed);
            }
        });
        return services;
    }

    private static IServiceCollection AddCorsPolicy(
        this IServiceCollection services,
        AppSettings appSettings)
    {
        var allowedOrigins = (appSettings.Cors.AllowedOrigins)
            .Where(origin =>
                !string.IsNullOrWhiteSpace(origin) &&
                !origin.Contains("${", StringComparison.Ordinal) &&
                Uri.TryCreate(origin, UriKind.Absolute, out _))
            .Select(origin => origin.Trim().TrimEnd('/'))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (allowedOrigins.Count == 0 &&
            !string.IsNullOrWhiteSpace(appSettings.FrontendBaseUrl) &&
            Uri.TryCreate(appSettings.FrontendBaseUrl, UriKind.Absolute, out _))
        {
            allowedOrigins.Add(appSettings.FrontendBaseUrl.Trim().TrimEnd('/'));
        }

        services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy.WithOrigins(allowedOrigins.ToArray())
                      .AllowAnyMethod()
                      .AllowAnyHeader()
                      .WithExposedHeaders("X-Correlation-ID", "Retry-After", "Content-Disposition");

                if (appSettings.Cors.AllowCredentials)
                {
                    policy.AllowCredentials();
                }
            });
        });

        return services;
    }
}
