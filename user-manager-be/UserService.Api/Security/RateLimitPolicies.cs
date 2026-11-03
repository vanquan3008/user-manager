using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using UserService.Application.Configuration;

namespace UserService.Api.Security;

public static class RateLimitPolicies
{
    /// <summary>Credential endpoints: login, registration, MFA, password reset, OAuth token.</summary>
    public const string Auth = "auth";
    /// <summary>Token refresh and logout (called automatically by clients).</summary>
    public const string Refresh = "refresh";

    public static IServiceCollection AddPlatformRateLimiting(this IServiceCollection services, AppSettings settings)
    {
        var config = settings.RateLimit;
        var whitelisted = (config.WhitelistedPaths ?? Array.Empty<string>())
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .Append("/health")
            .ToArray();

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.OnRejected = async (context, cancellationToken) =>
            {
                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                    context.HttpContext.Response.Headers.RetryAfter = ((int)Math.Ceiling(retryAfter.TotalSeconds)).ToString();

                context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>()
                    .CreateLogger("RateLimiting")
                    .LogWarning("Rate limit exceeded for {Ip} on {Method} {Path}",
                        ClientKey(context.HttpContext), context.HttpContext.Request.Method, context.HttpContext.Request.Path);

                await context.HttpContext.Response.WriteAsJsonAsync(new ApiResponse<object>(
                    false, null, "Too many requests",
                    new ErrorDetail("RATE_LIMITED", "Too many requests. Please wait and try again.")), cancellationToken);
            };

            // Global per-client limit.
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
            {
                var path = context.Request.Path.Value ?? string.Empty;
                if (whitelisted.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase)) ||
                    HttpMethods.IsOptions(context.Request.Method))
                    return RateLimitPartition.GetNoLimiter("unlimited");

                return RateLimitPartition.GetFixedWindowLimiter(ClientKey(context), _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = Math.Max(1, config.RequestLimit),
                    Window = TimeSpan.FromSeconds(Math.Max(1, config.WindowSeconds)),
                    QueueLimit = 0
                });
            });

            // Stricter limit for credential-handling endpoints (complements per-account lockout).
            options.AddPolicy(Auth, context => RateLimitPartition.GetFixedWindowLimiter(ClientKey(context), _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = Math.Max(1, config.AuthRequestLimit),
                Window = TimeSpan.FromSeconds(Math.Max(1, config.AuthWindowSeconds)),
                QueueLimit = 0
            }));

            options.AddPolicy(Refresh, context => RateLimitPartition.GetFixedWindowLimiter(ClientKey(context), _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = Math.Max(10, config.AuthRequestLimit * 3),
                Window = TimeSpan.FromSeconds(Math.Max(1, config.AuthWindowSeconds)),
                QueueLimit = 0
            }));
        });

        return services;
    }

    /// <summary>Client IP after the trusted forwarded-headers middleware ran.</summary>
    private static string ClientKey(HttpContext context)
        => context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
}
