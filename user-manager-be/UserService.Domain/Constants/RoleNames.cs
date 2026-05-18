namespace UserService.Domain.Constants;

public static class RoleNames
{
    public const string SuperAdmin = "SuperAdmin";
    public const string Admin = "Admin";
    public const string User = "User";
    public const string Guest = "Guest";
    public static readonly string[] AllRoles = 
    {
        SuperAdmin,
        Admin,
        User,
        Guest
    };
    public static readonly string[] SystemRoles = 
    {
        SuperAdmin,
        Admin,
        User,
        Guest
    };

    public static bool IsSuperAdmin(string? roleName)
        => string.Equals(roleName, SuperAdmin, StringComparison.OrdinalIgnoreCase);

    public static bool IsAdmin(string? roleName)
        => string.Equals(roleName, Admin, StringComparison.OrdinalIgnoreCase);

    public static bool ContainsSuperAdmin(IEnumerable<string>? roleNames)
        => roleNames?.Any(IsSuperAdmin) == true;

    public static bool ContainsAdmin(IEnumerable<string>? roleNames)
        => roleNames?.Any(IsAdmin) == true;

    public static bool ContainsSystemAdministrator(IEnumerable<string>? roleNames)
        => roleNames?.Any(roleName => IsAdmin(roleName) || IsSuperAdmin(roleName)) == true;
}
