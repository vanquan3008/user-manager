namespace UserService.Domain.Constants;

public static partial class PermissionCodes
{
    public const string ManageApplications = "APPLICATION.MANAGE";
    public const string ViewApplications = "APPLICATION.READ";
    public const string CreateApplication = "APPLICATION.CREATE";
    public const string EditApplication = "APPLICATION.UPDATE";
    public const string DeleteApplication = "APPLICATION.DELETE";
    public const string ApplicationsWildcard = "APPLICATION.*";
}
