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

        var allowedByPermissions = PermissionCodeMapper.IsGranted(permissions, requiredPermission);

        if (!allowedByPermissions)
            return false;

        var oauthScopes = ReadCsvClaim(user, JwtClaimNames.OAuthScopes);
        if (oauthScopes.Count == 0)
            return true;

        if (!HasPermissionLikeScopes(oauthScopes))
            return true;

        return PermissionCodeMapper.IsGranted(oauthScopes, requiredPermission);
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
