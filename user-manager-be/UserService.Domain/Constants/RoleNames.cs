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
}
