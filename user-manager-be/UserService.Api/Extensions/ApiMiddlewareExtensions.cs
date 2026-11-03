using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Serilog;
using UserService.Api.Middleware;
using UserService.Application.Configuration;
using UserService.Application.Interfaces.Services.Security;

namespace UserService.Api.Extensions;

public static class ApiMiddlewareExtensions
{
    public static WebApplication UseApiPipeline(
        this WebApplication app,
        AppSettings appSettings)
    {
        // Must run first so the client IP / scheme are correct for everything downstream.
        app.UseForwardedHeaders();

        if (!app.Environment.IsDevelopment())
        {
            app.UseHsts();
            app.UseHttpsRedirection();
        }

        if (appSettings.Swagger.Enabled)
        {
            if (appSettings.IsProduction() && !appSettings.Swagger.RequireAuthentication && appSettings.Swagger.AllowedIpAddresses.Count == 0)
            {
                Log.Warning("Swagger is enabled in production without authentication or IP restrictions. This is a security risk.");
            }
            
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "UserService API v1");
                options.RoutePrefix = "swagger";
                options.DocumentTitle = "UserService API Documentation";
            });

            app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();
        }
        else
        {
            app.MapGet("/", () => Results.Ok(new
            {
                service = appSettings.ApplicationName,
                version = appSettings.Version,
                status = "running"
            })).ExcludeFromDescription();
        }

        app.UseSerilogRequestLogging();
        app.UseCors();
        app.UseApiMiddlewares();
        app.UseRateLimiter();

        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        MapHealthChecks(app, appSettings);

        return app;
    }

    /// <summary>
    /// <c>/health/live</c>: process is up (no dependencies). <c>/health/ready</c> and <c>/health</c>:
    /// database reachable and a signing key is loaded.
    /// </summary>
    private static void MapHealthChecks(WebApplication app, AppSettings appSettings)
    {
        app.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = _ => false,
            ResponseWriter = (ctx, report) => WriteHealth(ctx, report, appSettings)
        }).WithTags("Health");

        foreach (var path in new[] { "/health/ready", "/health" })
        {
            app.MapHealthChecks(path, new HealthCheckOptions
            {
                Predicate = check => check.Tags.Contains("ready"),
                ResponseWriter = (ctx, report) => WriteHealth(ctx, report, appSettings)
            }).WithTags("Health");
        }
    }

    private static Task WriteHealth(HttpContext context, HealthReport report, AppSettings appSettings)
    {
        var keys = context.RequestServices.GetRequiredService<ISigningKeyProvider>().GetPublicJwks();
        var keysHealthy = keys.Count > 0;
        var status = report.Status == HealthStatus.Healthy && keysHealthy ? "healthy" : "unhealthy";
        if (status != "healthy")
            context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;

        context.Response.ContentType = "application/json";
        return context.Response.WriteAsync(JsonSerializer.Serialize(new
        {
            status,
            service = appSettings.ApplicationName,
            version = appSettings.Version,
            timestamp = DateTime.UtcNow,
            checks = report.Entries.ToDictionary(e => e.Key, e => e.Value.Status.ToString().ToLowerInvariant())
                .Append(new KeyValuePair<string, string>("signing_keys", keysHealthy ? "healthy" : "unhealthy"))
                .ToDictionary(kv => kv.Key, kv => kv.Value)
        }, new JsonSerializerOptions(JsonSerializerDefaults.Web)));
    }
}
