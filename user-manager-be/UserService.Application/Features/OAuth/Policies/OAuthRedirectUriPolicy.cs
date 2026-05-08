namespace UserService.Application.Features.OAuth.Policies;

public sealed class OAuthRedirectUriPolicy : IOAuthRedirectUriPolicy
{
    private readonly AppSettings _appSettings;

    public OAuthRedirectUriPolicy(AppSettings appSettings)
    {
        _appSettings = appSettings;
    }

    public bool IsAllowed(string redirectUri, string? appAllowedUris = null)
    {
        if (!Uri.TryCreate(redirectUri, UriKind.Absolute, out var uri))
            return false;
        var isLocalhost = uri.IsLoopback;
        var isHttps = string.Equals(
            uri.Scheme,
            Uri.UriSchemeHttps,
            StringComparison.OrdinalIgnoreCase);

        if (!isLocalhost && !isHttps)
            return false;
        if (isLocalhost &&
            !_appSettings.OAuth.AllowHttpForLocalhost &&
            !isHttps)
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(appAllowedUris))
        {
            var allowedUris = appAllowedUris.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            foreach (var allowed in allowedUris)
            {
                if (!Uri.TryCreate(allowed, UriKind.Absolute, out var allowedUri))
                    continue;

                if (IsExactMatch(uri, allowedUri))
                    return true;
            }
        }
        var allowedHosts = _appSettings.OAuth.AllowedRedirectUriHosts
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (allowedHosts.Count == 0)
            return false;

        return allowedHosts.Contains(uri.Host);
    }

    private static bool IsExactMatch(Uri actual, Uri allowed)
    {
        return
            string.Equals(actual.Scheme, allowed.Scheme, StringComparison.OrdinalIgnoreCase)
            && string.Equals(actual.Host, allowed.Host, StringComparison.OrdinalIgnoreCase)
            && actual.Port == allowed.Port
            && string.Equals(
                actual.AbsolutePath.TrimEnd('/'),
                allowed.AbsolutePath.TrimEnd('/'),
                StringComparison.OrdinalIgnoreCase);
    }
}

