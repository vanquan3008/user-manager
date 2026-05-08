namespace UserService.Application.Services;

public static class FilePublicUrlBuilder
{
    public static string ToClientAbsoluteUrl(string? url, string? apiPublicBaseUrl)
    {
        if (string.IsNullOrWhiteSpace(url))
            return url ?? string.Empty;

        var urltrim = url.Trim();
        if (urltrim.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ||
            urltrim.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
            return urltrim;

        var baseUrl = string.IsNullOrWhiteSpace(apiPublicBaseUrl)
            ? null
            : apiPublicBaseUrl.Trim().TrimEnd('/');

        if (baseUrl is null)
            return urltrim.StartsWith('/') ? urltrim : "/" + urltrim;

        if (!urltrim.StartsWith('/'))
            urltrim = "/" + urltrim;

        return baseUrl + urltrim;
    }
}
