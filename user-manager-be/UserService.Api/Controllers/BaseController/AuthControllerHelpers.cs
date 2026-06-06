using UserService.Application.Configuration;
using UserService.Application.Features.Auth.Commands;

namespace UserService.Api.Controllers.Helpers;

internal static class AuthControllerHelpers
{
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
            Path = "/"
        });
    }

    internal static void ClearRefreshTokenCookie(
        HttpRequest request,
        HttpResponse response,
        string refreshTokenCookieName)
    {
        response.Cookies.Delete(refreshTokenCookieName, new CookieOptions
        {
            HttpOnly = true,
            Secure = request.IsHttps,
            SameSite = request.IsHttps ? SameSiteMode.None : SameSiteMode.Lax,
            Path = "/"
        });
    }

    internal static AuthResponse ToClientAuthResponse(AuthResponse response)
        => response with { RefreshToken = null };
}

