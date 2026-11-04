using UserService.Application.Configuration;
using UserService.Application.Features.Auth.Commands;

namespace UserService.Api.Controllers.Helpers;

internal static class AuthControllerHelpers
{
    /// <summary>The refresh cookie is only ever sent to the auth endpoints that consume it.</summary>
    internal const string RefreshCookiePath = "/api/auth";
    internal const string CsrfHeaderName = "X-Requested-With";

    internal static string? ResolveRefreshToken(
        HttpRequest request,
        string? refreshTokenFromBody,
        string refreshTokenCookieName)
    {
        if (!string.IsNullOrWhiteSpace(refreshTokenFromBody))
            return refreshTokenFromBody.Trim();

        if (request.Cookies.TryGetValue(refreshTokenCookieName, out var cookieValue) &&
            !string.IsNullOrWhiteSpace(cookieValue))
        {
            return cookieValue.Trim();
        }

        return null;
    }

    /// <summary>
    /// Cookie-authenticated endpoints require a custom header. Browsers cannot add it cross-origin
    /// without a CORS preflight, which blocks cross-site request forgery.
    /// </summary>
    internal static bool HasCsrfHeader(HttpRequest request, bool usesCookie)
        => !usesCookie || request.Headers.ContainsKey(CsrfHeaderName);

    internal static void IssueRefreshTokenCookie(
        HttpRequest request,
        HttpResponse response,
        AppSettings appSettings,
        string refreshTokenCookieName,
        string? refreshToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            return;

        response.Cookies.Append(refreshTokenCookieName, refreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = request.IsHttps,
            SameSite = request.IsHttps ? SameSiteMode.None : SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.AddDays(appSettings.Jwt.RefreshTokenExpirationDays),
            IsEssential = true,
            Path = RefreshCookiePath
        });
    }

    internal static void ClearRefreshTokenCookie(
        HttpRequest request,
        HttpResponse response,
        string refreshTokenCookieName)
    {
        foreach (var path in new[] { RefreshCookiePath, "/" })
        {
            response.Cookies.Delete(refreshTokenCookieName, new CookieOptions
            {
                HttpOnly = true,
                Secure = request.IsHttps,
                SameSite = request.IsHttps ? SameSiteMode.None : SameSiteMode.Lax,
                Path = path
            });
        }
    }

    internal static AuthResponse ToClientAuthResponse(AuthResponse response)
        => response with { RefreshToken = null };
}
