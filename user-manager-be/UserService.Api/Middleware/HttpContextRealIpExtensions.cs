namespace UserService.Api.Middleware;

public static class HttpContextRealIpExtensions
{
    /// <summary>
    /// The client IP. <c>X-Forwarded-For</c> is only honoured when the request comes from a configured
    /// trusted proxy (see <c>AppSettings:Security:ForwardedHeaders</c>), so the value cannot be spoofed.
    /// </summary>
    public static string? GetRealIpAddress(this HttpContext context)
        => context.Connection.RemoteIpAddress?.IsIPv4MappedToIPv6 == true
            ? context.Connection.RemoteIpAddress.MapToIPv4().ToString()
            : context.Connection.RemoteIpAddress?.ToString();
}
