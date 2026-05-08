using System.Security.Cryptography;

namespace UserService.Application.Common;

public static class OAuthHelpers
{
    public static string GenerateCode(int entropyBytes = 64)
    {
        var bytes = RandomNumberGenerator.GetBytes(entropyBytes);
        return Base64UrlEncode(bytes);
    }

    public static string Base64UrlEncode(byte[] input)
    {
        return Convert.ToBase64String(input)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    public static bool IsValidBase64Url(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return false;
        for (int i = 0; i < value.Length; i++)
        {
            var c = value[i];
            if (!(c >= 'A' && c <= 'Z') && !(c >= 'a' && c <= 'z') && !(c >= '0' && c <= '9') && c != '-' && c != '_')
                return false;
        }
        return true;
    }
    public static string BuildRedirectUrl(string baseUrl, string code, string? state)
    {
        if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out _))
            throw new ArgumentException("Invalid redirect url");
        var uriBuilder = new UriBuilder(baseUrl);
        var query = System.Web.HttpUtility.ParseQueryString(uriBuilder.Query);

        query["code"] = code;
        if (!string.IsNullOrWhiteSpace(state))
        {
            query["state"] = state;
        }
        uriBuilder.Query = query.ToString();
        return uriBuilder.ToString();
    }
}

