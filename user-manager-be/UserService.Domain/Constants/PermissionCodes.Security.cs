namespace UserService.Domain.Constants;

public static partial class PermissionCodes
{
    /// <summary>View security configuration and signing keys.</summary>
    public const string ViewSecurity = "SECURITY.READ";
    /// <summary>Rotate/revoke signing keys and reset other users' MFA.</summary>
    public const string ManageSecurity = "SECURITY.MANAGE";
}
