using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;
using System.Text.Json;
using UserService.Api.Extensions;
using UserService.Domain.Constants;

namespace UserService.Api.Filters;

public sealed class ApplicationScopeActionFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(
        ActionExecutingContext context, 
        ActionExecutionDelegate next
    )
    {
        var http = context.HttpContext;
        var user = http.User;

        if (user.Identity?.IsAuthenticated != true)
        {
            await next();
            return;
        }
        if (user.IsInRole(RoleNames.SuperAdmin))
        {
            await next();
            return;
        }
        var path = http.Request.Path.Value ?? string.Empty;
        var isAppRoute = path.StartsWith("/api/applications/", StringComparison.OrdinalIgnoreCase);
        var isSubscriptionsRoute = path.StartsWith("/api/subscriptions", StringComparison.OrdinalIgnoreCase);
        if (!isAppRoute && !isSubscriptionsRoute)
        {
            await next();
            return;
        }
        Guid? targetAppId = null;
        if (isAppRoute)
        {
            if (context.RouteData.Values.TryGetValue("applicationId", out var routeVal)
                && Guid.TryParse(routeVal?.ToString(), out var routeAppId))
            {
                targetAppId = routeAppId;
            }
        }
        else if (isSubscriptionsRoute)
        {
            targetAppId = await ResolveSubscriptionApplicationIdAsync(http.Request);
            targetAppId ??= ResolveImplicitApplicationId(user);
        }

        if (!targetAppId.HasValue)
        {
            if (isSubscriptionsRoute)
            {
                throw new UnauthorizedAccessException("Missing application scope in request.");
            }

            await next();
            return;
        }

        var ssoAppIds = http.User.GetAuthorizedSSOApplicationIds();
        if (ssoAppIds.Count > 0)
        {
            if (!ssoAppIds.Contains(targetAppId.Value))
            {
                throw new UnauthorizedAccessException("SSO account does not have access to this application.");
            }

            await next();
            return;
        }

        if (http.User.TryGetTokenApplicationId() is not Guid tokenAppId)
            throw new UnauthorizedAccessException("Missing application scope in token.");

        if (tokenAppId != targetAppId.Value)
            throw new UnauthorizedAccessException("Token application_id does not match requested application.");

        await next();
    }

    private static Guid? ResolveImplicitApplicationId(ClaimsPrincipal user)
    {
        var ssoAppIds = user.GetAuthorizedSSOApplicationIds();
        if (ssoAppIds.Count == 1)
            return ssoAppIds.First();

        return user.TryGetTokenApplicationId();
    }

    private static async Task<Guid?> ResolveSubscriptionApplicationIdAsync(HttpRequest request)
    {
        var queryVal = request.Query["applicationId"].ToString();
        if (Guid.TryParse(queryVal, out var queryAppId))
            return queryAppId;

        if (!(HttpMethods.IsPost(request.Method) || HttpMethods.IsPut(request.Method) || HttpMethods.IsPatch(request.Method)))
            return null;

        if (request.ContentLength is null or <= 0 || request.Body.CanRead != true)
            return null;

        request.EnableBuffering();
        request.Body.Position = 0;

        try
        {
            using var json = await JsonDocument.ParseAsync(request.Body);
            if (json.RootElement.ValueKind != JsonValueKind.Object)
                return null;

            if (!TryGetJsonPropertyIgnoreCase(json.RootElement, "applicationId", out var appIdElement))
                return null;

            if (appIdElement.ValueKind == JsonValueKind.String
                && Guid.TryParse(appIdElement.GetString(), out var parsedStringGuid))
            {
                return parsedStringGuid;
            }

            if (appIdElement.ValueKind == JsonValueKind.Null)
                return null;

            return Guid.TryParse(appIdElement.ToString(), out var parsedGuid) ? parsedGuid : null;
        }
        catch (JsonException)
        {
            return null;
        }
        finally
        {
            request.Body.Position = 0;
        }
    }

    private static bool TryGetJsonPropertyIgnoreCase(JsonElement element, string propertyName, out JsonElement value)
    {
        foreach (var property in element.EnumerateObject())
        {
            if (string.Equals(property.Name, propertyName, StringComparison.OrdinalIgnoreCase))
            {
                value = property.Value;
                return true;
            }
        }

        value = default;
        return false;
    }
}

