using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;

namespace UserService.Api.Authorization;

/// <summary>
/// Produces consistent JSON 401/403 responses. An authenticated caller lacking a permission always
/// receives 403 (previously the OpenIddict scheme turned some forbids into 401 challenges).
/// </summary>
public sealed class ApiAuthorizationResultHandler : IAuthorizationMiddlewareResultHandler
{
    private readonly AuthorizationMiddlewareResultHandler _default = new();

    public async Task HandleAsync(RequestDelegate next, HttpContext context, AuthorizationPolicy policy, PolicyAuthorizationResult authorizeResult)
    {
        if (authorizeResult.Forbidden && context.User.Identity?.IsAuthenticated == true)
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsJsonAsync(new ApiResponse<object>(false, null, "Forbidden",
                new ErrorDetail("FORBIDDEN", "You do not have permission to perform this action.")));
            return;
        }

        if (authorizeResult.Challenged)
        {
            await _default.HandleAsync(next, context, policy, authorizeResult);
            if (!context.Response.HasStarted && context.Response.StatusCode == StatusCodes.Status401Unauthorized)
            {
                await context.Response.WriteAsJsonAsync(new ApiResponse<object>(false, null, "Unauthorized",
                    new ErrorDetail("UNAUTHORIZED", "Authentication is required or the access token is invalid.")));
            }
            return;
        }

        await _default.HandleAsync(next, context, policy, authorizeResult);
    }
}
