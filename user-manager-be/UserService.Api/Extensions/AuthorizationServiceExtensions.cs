using Microsoft.AspNetCore.Authorization;
using OpenIddict.Validation.AspNetCore;
using System.Security.Claims;
using UserService.Api.Authorization;
using UserService.Domain.Constants;

namespace UserService.Api.Extensions;

public static class AuthorizationServiceExtensions
{
    public static IServiceCollection AddCustomAuthorization(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            options.DefaultPolicy = new AuthorizationPolicyBuilder("Bearer", OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)
                .RequireAuthenticatedUser()
                .Build();

            options.AddPolicy(AuthorizationPolicies.CreateOrManageApplications, policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireAssertion(context =>
                    HasPermission(context.User, PermissionCodes.CreateApplication) ||
                    HasPermission(context.User, PermissionCodes.ManageApplications));
            });

        });
        
        services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
        services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();

        return services;
    }

    private static bool HasPermission(ClaimsPrincipal user, string permissionCode)
    {
        var permissions = user.FindFirst(JwtClaimNames.Permissions)?.Value;
        if (string.IsNullOrWhiteSpace(permissions))
            return false;

        var grantedPermissions = permissions
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        return PermissionCodeMapper.IsGranted(grantedPermissions, permissionCode) ||
               PermissionCodeMapper.IsGranted(grantedPermissions, PermissionCodes.ApplicationsWildcard);
    }

}
