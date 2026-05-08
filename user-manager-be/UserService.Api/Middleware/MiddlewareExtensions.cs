using UserService.Api.Middleware.Options;

namespace UserService.Api.Middleware;
public static class MiddlewareExtensions
{
    public static IApplicationBuilder UseApiMiddlewares(this IApplicationBuilder app)
    {
        return app
            .UseRequestLogging()
            .UseExceptionHandling();
    }
    public static IApplicationBuilder UseRealIp(
        this IApplicationBuilder app,
        Action<RealIpOptions>? configure = null) 
        => app.UseMiddleware<RealIpMiddleware>(CreateOptions(configure));

    private static IApplicationBuilder UseRequestLogging(this IApplicationBuilder app) 
        => app.UseMiddleware<RequestLoggingMiddleware>();

    public static IApplicationBuilder UseRateLimiting(
        this IApplicationBuilder app,
        Action<RateLimitOptions>? configure = null) 
        => app.UseMiddleware<RateLimitingMiddleware>(CreateOptions(configure));

    private static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder app) 
        => app.UseMiddleware<ExceptionHandlingMiddleware>();

    private static T CreateOptions<T>(Action<T>? configure) where T : new()
    {
        var options = new T();
        configure?.Invoke(options);
        return options;
    }
}

