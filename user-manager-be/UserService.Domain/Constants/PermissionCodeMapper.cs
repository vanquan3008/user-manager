namespace UserService.Domain.Constants;

public static class PermissionCodeMapper
{
    public static IEnumerable<string> GetEquivalentCodes(string permissionCode)
    {
        if (!string.IsNullOrWhiteSpace(permissionCode))
            yield return permissionCode.Trim();
    }

    public static IReadOnlySet<string> NormalizeGrantedPermissions(IEnumerable<string> permissionCodes)
    {
        return permissionCodes
            .SelectMany(GetEquivalentCodes)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    public static bool IsGranted(IEnumerable<string> grantedPermissions, string requiredPermission)
    {
        var normalizedGranted = NormalizeGrantedPermissions(grantedPermissions);
        var requiredCodes = GetEquivalentCodes(requiredPermission).ToArray();

        foreach (var requiredCode in requiredCodes)
        {
            if (normalizedGranted.Contains(requiredCode) || HasWildcardPermission(normalizedGranted, requiredCode))
                return true;
        }

        return false;
    }

    private static bool HasWildcardPermission(IReadOnlySet<string> grantedPermissions, string requiredPermission)
    {
        var separatorIndex = requiredPermission.IndexOf('.');
        if (separatorIndex <= 0)
            return false;

        var wildcardPermission = requiredPermission[..separatorIndex] + ".*";
        return grantedPermissions.Contains(wildcardPermission);
    }

    public static IReadOnlyCollection<string> GetCanonicalCodes()
    {
        return typeof(PermissionCodes)
            .GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
            .Where(field => field.FieldType == typeof(string))
            .Select(field => field.GetValue(null) as string)
            .Where(code => !string.IsNullOrWhiteSpace(code))
            .Select(code => code!)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }
}
