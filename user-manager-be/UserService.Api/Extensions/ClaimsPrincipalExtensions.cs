using System.Security.Claims;

using UserService.Domain.Constants;

namespace UserService.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid? TryGetCurrentUserId(this ClaimsPrincipal? principal)
    {
        var v = principal?.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(v, out var id) ? id : null;
    }

    public static Guid GetRequiredCurrentUserId(this ClaimsPrincipal? principal)
    {
        var userId = principal.TryGetCurrentUserId();
        if (userId is null)
            throw new UnauthorizedAccessException("Missing authenticated user identifier.");

        return userId.Value;
    }

    public static Guid? TryGetTokenApplicationId(this ClaimsPrincipal? principal)
    {
        var appIdRaw = principal?.FindFirst(JwtClaimNames.ApplicationId)?.Value;
        return Guid.TryParse(appIdRaw, out var appId) ? appId : null;
    }

    public static HashSet<Guid> GetAuthorizedSsoApplicationIds(this ClaimsPrincipal? principal)
    {
        var raw = principal?.FindFirst(JwtClaimNames.SsoAppIds)?.Value;
        var result = new HashSet<Guid>();
        if (string.IsNullOrWhiteSpace(raw))
            return result;

        foreach (var segment in raw.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (Guid.TryParse(segment, out var id))
                result.Add(id);
        }

        return result;
    }
}

