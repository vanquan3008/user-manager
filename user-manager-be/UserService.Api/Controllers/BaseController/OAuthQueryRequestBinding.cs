using Microsoft.Extensions.Primitives;
using UserService.Api.Contracts.OAuth;

namespace UserService.Api.Controllers.Helpers;

internal static class OAuthQueryRequestBinding
{
    internal static OAuthAuthorizeRequest ToAuthorizeRequest(
        OAuthAuthorizeQueryRequest request,
        IQueryCollection query)
    {
        var clientId = FirstNonEmpty(request.ClientId, ReadQueryValue(query, "client_id", "clientId"));
        var redirectUri = FirstNonEmpty(request.RedirectUri, ReadQueryValue(query, "redirect_uri", "redirectUri"));
        var scope = FirstNonEmpty(request.Scope, ReadQueryValue(query, "scope"));
        var state = FirstNonEmpty(request.State, ReadQueryValue(query, "state"));
        var codeChallenge = FirstNonEmpty(request.CodeChallenge, ReadQueryValue(query, "code_challenge", "codeChallenge"));
        var codeChallengeMethod = FirstNonEmpty(
            request.CodeChallengeMethod,
            ReadQueryValue(query, "code_challenge_method", "codeChallengeMethod"),
            "S256");
        var prompt = FirstNonEmpty(request.Prompt, ReadQueryValue(query, "prompt"));
        var scopeValues = ReadQueryValues(query, "scopes", "scope", "Scopes");
        var scopes = request.Scopes is { Length: > 0 }
            ? request.Scopes
            : ParseScopes(scope, scopeValues);

        return new OAuthAuthorizeRequest(
            ClientId: clientId ?? string.Empty,
            RedirectUri: redirectUri ?? string.Empty,
            Scope: scope,
            State: state,
            CodeChallenge: codeChallenge,
            CodeChallengeMethod: codeChallengeMethod,
            Prompt: prompt,
            Scopes: scopes);
    }

    internal static OAuthConsentRequest ToConsentRequest(
        OAuthAuthorizeQueryRequest request,
        IQueryCollection query,
        bool approve)
    {
        var authorizeRequest = ToAuthorizeRequest(request, query);
        var scopes = authorizeRequest.Scopes;
        if (scopes == null || scopes.Length == 0)
        {
            scopes = ParseScopes(authorizeRequest.Scope, ReadQueryValues(query, "scopes", "scope", "Scopes"));
        }

        return new OAuthConsentRequest(
            ClientId: authorizeRequest.ClientId,
            RedirectUri: authorizeRequest.RedirectUri,
            Scopes: scopes ?? Array.Empty<string>(),
            State: authorizeRequest.State,
            CodeChallenge: authorizeRequest.CodeChallenge ?? string.Empty,
            CodeChallengeMethod: authorizeRequest.CodeChallengeMethod ?? "S256",
            Approve: approve);
    }

    private static string? ReadQueryValue(IQueryCollection query, string key)
    {
        return ReadQueryValue(query, [key]);
    }

    private static string? ReadQueryValue(IQueryCollection query, params string[] keys)
    {
        foreach (var key in keys)
        {
            if (!query.TryGetValue(key, out var values))
            {
                continue;
            }

            foreach (var value in values)
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    return value.Trim();
                }
            }
        }

        return null;
    }

    private static StringValues ReadQueryValues(IQueryCollection query, params string[] keys)
    {
        var values = new List<string>();

        foreach (var key in keys)
        {
            if (!query.TryGetValue(key, out var rawValues))
            {
                continue;
            }

            foreach (var rawValue in rawValues)
            {
                if (!string.IsNullOrWhiteSpace(rawValue))
                {
                    values.Add(rawValue);
                }
            }
        }

        return values.Count == 0 ? StringValues.Empty : new StringValues(values.ToArray());
    }

    private static string? FirstNonEmpty(params string?[] values)
        => values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value))?.Trim();

    private static string[]? ParseScopes(string? scope, StringValues scopeValues)
    {
        var values = new List<string>();

        if (!string.IsNullOrWhiteSpace(scope))
        {
            values.AddRange(SplitScopes(scope));
        }

        foreach (var raw in scopeValues)
        {
            if (!string.IsNullOrWhiteSpace(raw))
            {
                values.AddRange(SplitScopes(raw));
            }
        }

        return values.Count == 0
            ? null
            : values
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
    }

    private static IEnumerable<string> SplitScopes(string raw)
        => raw
            .Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
}
