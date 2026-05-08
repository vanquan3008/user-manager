using System.Collections.Concurrent;

namespace UserService.Api.Middleware;
public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RateLimitingMiddleware> _logger;
    private readonly RateLimitOptions _options;
    private static readonly ConcurrentDictionary<string, ClientRateLimit> ClientLimits = new();

    public RateLimitingMiddleware(
        RequestDelegate next,
        ILogger<RateLimitingMiddleware> logger,
        RateLimitOptions? options = null)
    {
        _next = next;
        _logger = logger;
        _options = options ?? new RateLimitOptions();
        _ = StartCleanupTask();
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (IsWhitelisted(context))
        {
            await _next(context);
            return;
        }

        var clientId = GetClientIdentifier(context);
        var now = DateTime.UtcNow;

        var rateLimit = ClientLimits.GetOrAdd(clientId, _ => new ClientRateLimit
        {
            WindowStart = now,
            RequestCount = 0
        });

        var (isAllowed, retryAfter, remaining) = CheckRateLimit(rateLimit, now);
        AddRateLimitHeaders(context, remaining, retryAfter);

        if (!isAllowed)
        {
            await HandleRateLimitExceeded(context, clientId, retryAfter);
            return;
        }

        await _next(context);
    }

    private (bool IsAllowed, int RetryAfter, int Remaining) CheckRateLimit(
        ClientRateLimit rateLimit, 
        DateTime now)
    {
        lock (rateLimit)
        {
            // Reset window if expired
            if ((now - rateLimit.WindowStart).TotalSeconds >= _options.WindowSeconds)
            {
                rateLimit.WindowStart = now;
                rateLimit.RequestCount = 0;
            }

            rateLimit.RequestCount++;

            var remaining = Math.Max(0, _options.RequestLimit - rateLimit.RequestCount);
            var isAllowed = rateLimit.RequestCount <= _options.RequestLimit;
            var retryAfter = isAllowed 
                ? 0 
                : (int)(_options.WindowSeconds - (now - rateLimit.WindowStart).TotalSeconds);

            return (isAllowed, retryAfter, remaining);
        }
    }

    private bool IsWhitelisted(HttpContext context)
    {
        var path = context.Request.Path.Value ?? "";
        if (_options.WhitelistedPaths.Any(wp => path.StartsWith(wp, StringComparison.OrdinalIgnoreCase)))
        {
            return true;
        }
        var clientIp = context.GetRealIpAddress();
        return !string.IsNullOrEmpty(clientIp) && _options.WhitelistedIps.Contains(clientIp);
    }

    private string GetClientIdentifier(HttpContext context)
    {
        // Use real IP from Nginx headers
        var ip = context.GetRealIpAddress() 
            ?? context.Connection.RemoteIpAddress?.ToString() 
            ?? "unknown";
        
        // Optionally include User-Agent for more granular limiting
        if (_options.IncludeUserAgentInKey)
        {
            var userAgent = context.Request.Headers.UserAgent.ToString();
            return $"{ip}|{userAgent.GetHashCode()}";
        }

        return $"ip:{ip}";
    }

    private void AddRateLimitHeaders(HttpContext context, int remaining, int retryAfter)
    {
        context.Response.Headers.TryAdd("X-RateLimit-Limit", _options.RequestLimit.ToString());
        context.Response.Headers.TryAdd("X-RateLimit-Remaining", remaining.ToString());
        context.Response.Headers.TryAdd("X-RateLimit-Window", $"{_options.WindowSeconds}s");
        
        if (retryAfter > 0)
        {
            context.Response.Headers.TryAdd("Retry-After", retryAfter.ToString());
        }
    }

    private async Task HandleRateLimitExceeded(HttpContext context, string clientId, int retryAfter)
    {
        _logger.LogWarning(
            "Rate limit exceeded for client {ClientId}. Method: {Method}, Path: {Path}",
            clientId,
            context.Request.Method,
            context.Request.Path);

        context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        context.Response.ContentType = "application/json";

        var response = new
        {
            statusCode = 429,
            title = "Too Many Requests",
            message = "Rate limit exceeded. Please try again later.",
            retryAfter = retryAfter,
            limit = _options.RequestLimit,
            window = $"{_options.WindowSeconds} seconds"
        };

        await context.Response.WriteAsJsonAsync(response);
    }

    private async Task StartCleanupTask()
    {
        while (true)
        {
            try
            {
                await Task.Delay(TimeSpan.FromMinutes(_options.CleanupIntervalMinutes));
                
                var now = DateTime.UtcNow;
                var keysToRemove = ClientLimits
                    .Where(kvp => (now - kvp.Value.WindowStart).TotalMinutes > _options.CleanupIntervalMinutes)
                    .Select(kvp => kvp.Key)
                    .ToList();

                foreach (var key in keysToRemove)
                {
                    ClientLimits.TryRemove(key, out _);
                }

                if (keysToRemove.Count > 0)
                {
                    _logger.LogDebug(
                        "Rate limit cleanup: Removed {Count} expired client entries",
                        keysToRemove.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in rate limit cleanup task");
            }
        }
    }

    private class ClientRateLimit
    {
        public DateTime WindowStart { get; set; }
        public int RequestCount { get; set; }
    }
}

/// <summary>
/// Configuration options for rate limiting
/// </summary>
public class RateLimitOptions
{
    public int RequestLimit { get; set; } = 100;
    public int WindowSeconds { get; set; } = 60;
    public bool IncludeUserAgentInKey { get; set; } = false;

    public int CleanupIntervalMinutes { get; set; } = 5;
    public HashSet<string> WhitelistedPaths { get; set; } = new() 
    { 
        "/health", 
        "/metrics" 
    };
    public HashSet<string> WhitelistedIps { get; set; } = new()
    {
        "127.0.0.1",
        "::1"
    };
}
