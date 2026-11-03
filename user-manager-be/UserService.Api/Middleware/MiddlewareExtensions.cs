namespace UserService.Api.Middleware;

public static class MiddlewareExtensions
{
    public static IApplicationBuilder UseApiMiddlewares(this IApplicationBuilder app)
        => app
            .UseMiddleware<SecurityHeadersMiddleware>()
            .UseMiddleware<RequestLoggingMiddleware>()
            .UseMiddleware<ExceptionHandlingMiddleware>();
}
