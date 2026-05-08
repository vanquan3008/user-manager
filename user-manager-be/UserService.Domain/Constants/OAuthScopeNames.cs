namespace UserService.Domain.Constants;

public static class OAuthScopeNames
{
    public static readonly string[] DefaultScopes =
    {
        "openid",
        "profile",
        "email"
    };

    public static IReadOnlyList<string> FilterDefaultScopes(IEnumerable<string>? scopes)
    {
        if (scopes == null)
            return Array.Empty<string>();

        var allowed = new HashSet<string>(DefaultScopes, StringComparer.OrdinalIgnoreCase);
        return scopes
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Select(s => s.Trim())
            .Where(s => allowed.Contains(s))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }
}
