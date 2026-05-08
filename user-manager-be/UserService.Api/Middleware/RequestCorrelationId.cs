namespace UserService.Api.Middleware;

public static class RequestCorrelationId
{
    public const string CorrelationIdHeaderName = "X-Correlation-ID";
    public const string RequestIdHeaderName = "X-Request-ID";
    private const int MaxIncomingIdLength = 128;
    public static string Resolve(HttpRequest request)
    {
        foreach (var headerName in new[] { CorrelationIdHeaderName, RequestIdHeaderName })
        {
            if (!request.Headers.TryGetValue(headerName, out var values) || values.Count == 0)
                continue;
            var raw = values[0]!.Trim();
            if (!TryNormalizeIncomingCorrelationId(raw, out var normalized))
                continue;
            return normalized;
        }

        return GenerateRequestId();
    }

    private static bool TryNormalizeIncomingCorrelationId(string? raw, out string normalized)
    {
        normalized = string.Empty;
        if (string.IsNullOrEmpty(raw) || raw.Length > MaxIncomingIdLength)
            return false;
        foreach (var c in raw)
        {
            if (char.IsControl(c))
                return false;
        }

        normalized = raw;
        return true;
    }

    private static string GenerateRequestId()
    {
        return Guid.NewGuid().ToString("N")[..12];
    }
}
