using Microsoft.OpenApi.Models;
using UserService.Api.Filters;

namespace UserService.Api.Extensions;

public static class SwaggerServiceExtensions
{
    public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.MapType<IFormFile>(() => new OpenApiSchema
            {
                Type = "string",
                Format = "binary"
            });

            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "UserService API",
                Version = "v1",
                Description =
                    "REST API for user management with magic link authentication.\n\n" +
                    "**Correlation (optional):** Send `X-Correlation-ID` or `X-Request-ID` on requests to tie logs across services; " +
                    "the API uses that value if valid (max 128 chars, no control characters), otherwise generates one. " +
                    "Both headers are returned on the response with the effective id.",
                Contact = new OpenApiContact
                {
                    Name = "UserService Team"
                }
            });

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token.",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
            options.OperationFilter<AuthorizeRolesOperationFilter>();
        });
        return services;
    }
}
