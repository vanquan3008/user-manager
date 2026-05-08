using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using UserService.Domain.Constants;

namespace UserService.Api.Authorization;

public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        if (context.User.IsInRole(RoleNames.Admin) || context.User.IsInRole(RoleNames.SuperAdmin))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }
 
        if (HasEffectivePermission(context.User, requirement.Permission))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        context.Fail();
        return Task.CompletedTask;
    }

    private static bool HasEffectivePermission(ClaimsPrincipal user, string requiredPermission)
    {
        var permissions = ReadCsvClaim(user, JwtClaimNames.Permissions);
        if (permissions.Count == 0)
            return false;

        var allowedByPermissions = permissions.Contains(requiredPermission, StringComparer.OrdinalIgnoreCase)
            || HasWildcardPermission(permissions, requiredPermission);

        if (!allowedByPermissions)
            return false;

        var oauthScopes = ReadCsvClaim(user, JwtClaimNames.OAuthScopes);
        if (oauthScopes.Count == 0)
            return true;

        if (!HasPermissionLikeScopes(oauthScopes))
            return true;

        return oauthScopes.Contains(requiredPermission, StringComparer.OrdinalIgnoreCase)
            || HasWildcardPermission(oauthScopes, requiredPermission);
    }

    private static bool HasWildcardPermission(IReadOnlyCollection<string> items, string requiredPermission)
    {
        var requiredResource = requiredPermission.Split('.')[0];
        var wildcardPermission = $"{requiredResource}.*";
        return items.Contains(wildcardPermission, StringComparer.OrdinalIgnoreCase);
    }

    private static HashSet<string> ReadCsvClaim(ClaimsPrincipal user, string claimType)
    {
        var claim = user.FindFirst(claimType);
        if (claim is null || string.IsNullOrWhiteSpace(claim.Value))
            return new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        return claim.Value
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    private static bool HasPermissionLikeScopes(IReadOnlyCollection<string> scopes)
    {
        foreach (var scope in scopes)
        {
            if (scope.Contains('.', StringComparison.Ordinal) || scope.EndsWith(".*", StringComparison.Ordinal))
                return true;
        }

        return false;
    }
}
