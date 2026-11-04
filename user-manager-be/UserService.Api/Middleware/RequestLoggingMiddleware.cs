using System.Diagnostics;

namespace UserService.Api.Middleware;

/// <summary>
/// Request logging and timing. Resolves correlation id from incoming headers when present,
/// otherwise generates one; echoes on response for distributed tracing.
/// </summary>
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var requestId = RequestCorrelationId.Resolve(context.Request);
        var stopwatch = Stopwatch.StartNew();

        context.Items["RequestId"] = requestId;
        context.Items["CorrelationId"] = requestId;
        context.Response.OnStarting(() =>
        {
            context.Response.Headers.TryAdd(RequestCorrelationId.RequestIdHeaderName, requestId);
            context.Response.Headers.TryAdd(RequestCorrelationId.CorrelationIdHeaderName, requestId);
            return Task.CompletedTask;
        });

        LogRequestStarted(context, requestId);

        using var _ = Serilog.Context.LogContext.PushProperty("CorrelationId", requestId);
        try
        {
            await _next(context);
            
            stopwatch.Stop();
            LogRequestCompleted(context, requestId, stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            LogRequestFailed(context, requestId, stopwatch.ElapsedMilliseconds, ex);
            throw; // Re-throw to let ExceptionHandlingMiddleware handle it
        }
    }

    private void LogRequestStarted(HttpContext context, string requestId)
    {
        _logger.LogInformation(
            "[{RequestId}] {Method} {Scheme}://{Host}{Path} started from {RemoteIp}",
            requestId,
            context.Request.Method,
            context.Request.Scheme,
            context.Request.Host,
            context.Request.Path,
            context.Connection.RemoteIpAddress);
    }

    private void LogRequestCompleted(HttpContext context, string requestId, long elapsedMs)
    {
        var logLevel = DetermineLogLevel(context.Response.StatusCode);
        
        _logger.Log(
            logLevel,
            "[{RequestId}] {Method} {Path} completed with {StatusCode} in {ElapsedMs}ms",
            requestId,
            context.Request.Method,
            context.Request.Path,
            context.Response.StatusCode,
            elapsedMs);

        // Log slow requests
        if (elapsedMs > 1000)
        {
            _logger.LogWarning(
                "[{RequestId}] SLOW REQUEST: {Method} {Path} took {ElapsedMs}ms",
                requestId,
                context.Request.Method,
                context.Request.Path,
                elapsedMs);
        }
    }

    private void LogRequestFailed(HttpContext context, string requestId, long elapsedMs, Exception exception)
    {
        _logger.LogError(
            exception,
            "[{RequestId}] {Method} {Path} failed after {ElapsedMs}ms with exception: {ExceptionType}",
            requestId,
            context.Request.Method,
            context.Request.Path,
            elapsedMs,
            exception.GetType().Name);
    }

    private static LogLevel DetermineLogLevel(int statusCode)
    {
        return statusCode switch
        {
            >= 500 => LogLevel.Error,
            >= 400 => LogLevel.Warning,
            _ => LogLevel.Information
        };
    }
}
