using System.Net;
using UserService.Api.Middleware.Options;

namespace UserService.Api.Middleware;

/// <summary>
/// Extracts real client IP from reverse proxy headers (Nginx)
/// </summary>
public class RealIpMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RealIpMiddleware> _logger;
    private readonly RealIpOptions _options;

    public RealIpMiddleware(
        RequestDelegate next,
        ILogger<RealIpMiddleware> logger,
        RealIpOptions? options = null)
    {
        _next = next;
        _logger = logger;
        _options = options ?? new RealIpOptions();
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var realIp = ExtractRealIpAddress(context);
        
        if (!string.IsNullOrEmpty(realIp))
        {
            StoreRealIpInContext(context, realIp);
            
            if (_options.LogIpExtraction)
            {
                _logger.LogDebug(
                    "Real IP extracted: {RealIp} (Original: {OriginalIp}, User-Agent: {UserAgent})",
                    realIp,
                    context.Connection.RemoteIpAddress,
                    context.Request.Headers.UserAgent.ToString());
            }
        }

        await _next(context);
    }

    private string? ExtractRealIpAddress(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue("X-Real-IP", out var realIpHeader))
        {
            var realIp = realIpHeader.ToString();
            if (IsValidIpAddress(realIp))
            {
                return realIp;
            }
        }
        if (context.Request.Headers.TryGetValue("X-Forwarded-For", out var forwardedHeader))
        {
            var forwarded = forwardedHeader.ToString();
            var ips = forwarded.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            
            if (ips.Length > 0 && IsValidIpAddress(ips[0]))
            {
                return ips[0];
            }
        }
        return context.Connection.RemoteIpAddress?.ToString();
    }

    private static bool IsValidIpAddress(string ipAddress)
    {
        return IPAddress.TryParse(ipAddress, out _);
    }

    private static void StoreRealIpInContext(HttpContext context, string realIp)
    {
        context.Items["RealIpAddress"] = realIp;
        context.TraceIdentifier = $"{context.TraceIdentifier}|{realIp}";
    }
}

