using UserService.Api.Filters;
using UserService.Application.Configuration;
using UserService.Application.DTOs.Common;
using Microsoft.AspNetCore.Mvc;
using UserService.Api.Authorization.Services;
using FluentValidation;

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
            .ToArray();

        services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy.WithOrigins(allowedOrigins)
                      .AllowAnyMethod()
                      .AllowAnyHeader();

                if (appSettings.Cors.AllowCredentials)
                {
                    policy.AllowCredentials();
                }
            });
        });

        return services;
    }
}
