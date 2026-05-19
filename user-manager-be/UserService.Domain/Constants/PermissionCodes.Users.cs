namespace UserService.Domain.Constants;

public static partial class PermissionCodes
{
    public const string ManageUsers = "USER.MANAGE";
    public const string ViewUsers = "USER.READ";
    public const string CreateUser = "USER.CREATE";
    public const string EditUser = "USER.UPDATE";
    public const string DeleteUser = "USER.DELETE";

    public const string ViewMembers = "MEMBER.READ";
    public const string InviteMember = "MEMBER.INVITE";
    public const string RemoveMember = "MEMBER.REMOVE";
}
