namespace UserService.Domain.Constants;

public static class FeatureCodes
{
    public const string UserManagement = "USER_MANAGEMENT";
    public const string Billing = "BILLING";
    public const string Invoice = "INVOICE";
    public const string Reports = "REPORTS";
    public const string AuditLog = "AUDIT_LOG";
    public const string Analytics = "ANALYTICS";
    public const string Support = "SUPPORT";
    public const string AdvancedSecurity = "ADVANCED_SECURITY";
    public const string CustomDomain = "CUSTOM_DOMAIN";

    public static readonly IReadOnlyCollection<string> All = new[]
    {
        UserManagement,
        Billing,
        Invoice,
        Reports,
        AuditLog,
        Analytics,
        Support,
        AdvancedSecurity,
        CustomDomain
    };
}
