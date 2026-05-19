using UserService.Domain.Constants;

namespace UserService.Api.Authorization;

public static class AuthorizationPolicies
{
    public const string PermissionPolicyPrefix = "HasPermission:";
    // User Management
    public const string ManageUsers = PermissionPolicyPrefix + PermissionCodes.ManageUsers;
    public const string ViewUsers = PermissionPolicyPrefix + PermissionCodes.ViewUsers;
    public const string CreateUser = PermissionPolicyPrefix + PermissionCodes.CreateUser;
    public const string EditUser = PermissionPolicyPrefix + PermissionCodes.EditUser;
    public const string DeleteUser = PermissionPolicyPrefix + PermissionCodes.DeleteUser;
    // Role Management
    public const string ManageRoles = PermissionPolicyPrefix + PermissionCodes.ManageRoles;
    public const string ViewRoles = PermissionPolicyPrefix + PermissionCodes.ViewRoles;
    public const string CreateRole = PermissionPolicyPrefix + PermissionCodes.CreateRole;
    public const string EditRole = PermissionPolicyPrefix + PermissionCodes.EditRole;
    public const string DeleteRole = PermissionPolicyPrefix + PermissionCodes.DeleteRole;
    // Permission Management
    public const string ManagePermissions = PermissionPolicyPrefix + PermissionCodes.ManagePermissions;
    public const string ViewPermissions = PermissionPolicyPrefix + PermissionCodes.ViewPermissions;
    public const string CreatePermission = PermissionPolicyPrefix + PermissionCodes.CreatePermission;
    // Application Management
    public const string ManageApplications = PermissionPolicyPrefix + PermissionCodes.ManageApplications;
    public const string ViewApplications = PermissionPolicyPrefix + PermissionCodes.ViewApplications;
    public const string CreateApplication = PermissionPolicyPrefix + PermissionCodes.CreateApplication;
    public const string CreateOrManageApplications = "CreateOrManageApplications";
    // Reporting
    public const string ViewReports = PermissionPolicyPrefix + PermissionCodes.ViewReports;
    public const string ExportReports = PermissionPolicyPrefix + PermissionCodes.ExportReports;
    // Audit & Logs
    public const string ViewAuditLogs = PermissionPolicyPrefix + PermissionCodes.ViewAuditLogs;
    // Subscriptions catalog and plan operations
    public const string ViewSubscriptions = PermissionPolicyPrefix + PermissionCodes.ViewSubscriptions;
    public const string ManageSubscriptions = PermissionPolicyPrefix + PermissionCodes.ManageSubscriptions;
    // Subscription operations
    public const string ActivateSubscription = PermissionPolicyPrefix + PermissionCodes.ActivateSubscription;
    public const string UpgradeSubscription = PermissionPolicyPrefix + PermissionCodes.UpgradeSubscription;
    public const string ExtendSubscription = PermissionPolicyPrefix + PermissionCodes.ExtendSubscription;
    public const string CancelSubscription = PermissionPolicyPrefix + PermissionCodes.CancelSubscription;
    public static string FromPermissionCode(string permissionCode) => PermissionPolicyPrefix + permissionCode;

    public static string ExtractPermissionCode(string policyName)
        => policyName.StartsWith(PermissionPolicyPrefix, StringComparison.OrdinalIgnoreCase)
            ? policyName[PermissionPolicyPrefix.Length..]
            : policyName;
}

