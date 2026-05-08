using Serilog;
using UserService.Api.Middleware;
using UserService.Application.Configuration;

namespace UserService.Api.Extensions;

public static class ApiMiddlewareExtensions
{
    public static WebApplication UseApiPipeline(
        this WebApplication app,
        AppSettings appSettings)
    {
        // Configure forwarded headers for proxy/load balancer scenarios
        // This ensures request.IsHttps works correctly behind reverse proxies
        app.UseForwardedHeaders(new Microsoft.AspNetCore.Builder.ForwardedHeadersOptions
        {
            ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor 
                             | Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto,
            // Clear known networks and proxies to accept headers from any proxy
            // In production, configure specific proxy IPs for security
            KnownIPNetworks = { },
            KnownProxies = { }
        });
        
        if (!app.Environment.IsDevelopment())
        {
            app.UseHsts();
            app.UseHttpsRedirection();
        }

        // Swagger configuration - conditional based on settings
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

            app.MapGet("/", () => Results.Redirect("/swagger"));
            app.MapGet("/test-api", () => Results.Redirect("/swagger"));
        }
        else
        {
            app.MapGet("/", () => Results.Ok(new
            {
                service = appSettings.ApplicationName,
                version = appSettings.Version,
                status = "running"
            }));
        }

        app.UseSerilogRequestLogging();
        app.UseCors();
        app.UseStaticFiles();
        app.UseApiMiddlewares();
        app.UseRealIp(options =>
        {
            options.LogIpExtraction = app.Environment.IsDevelopment();
        });
        app.UseRateLimiting(options =>
        {
            options.RequestLimit = appSettings.RateLimit.RequestLimit;
            options.WindowSeconds = appSettings.RateLimit.WindowSeconds;

            if (appSettings.RateLimit.WhitelistedPaths != null)
            {
                foreach (var path in appSettings.RateLimit.WhitelistedPaths)
                {
                    options.WhitelistedPaths.Add(path);
                }
            }
        });

        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        app.MapGet("/health", () => Results.Ok(new
        {
            status = "healthy",
            service = appSettings.ApplicationName,
            timestamp = DateTime.UtcNow,
            environment = appSettings.Environment
        })).WithTags("Health");

        return app;
    }
}
