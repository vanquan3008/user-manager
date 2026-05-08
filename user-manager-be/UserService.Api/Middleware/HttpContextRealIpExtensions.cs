namespace UserService.Api.Middleware;

public static class HttpContextRealIpExtensions
{
    private const string RealIpKey = "RealIpAddress";

    public static string? GetRealIpAddress(this HttpContext context)
    {
        if (context.Items.TryGetValue(RealIpKey, out var ipAddress))
        {
            return ipAddress?.ToString();
        }
        
        return context.Connection.RemoteIpAddress?.ToString();
    }

    public static string GetRequiredRealIpAddress(this HttpContext context)
    {
        return context.GetRealIpAddress()
            ?? throw new InvalidOperationException("Real IP address not found in context");
    }

    public static bool IsProxied(this HttpContext context)
    {
        return context.Request.Headers.ContainsKey("X-Real-IP")
            || context.Request.Headers.ContainsKey("X-Forwarded-For");
    }
}
