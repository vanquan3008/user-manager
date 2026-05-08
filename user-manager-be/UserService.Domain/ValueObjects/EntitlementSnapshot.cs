namespace UserService.Domain.ValueObjects;

public sealed record EntitlementSnapshot(
    IReadOnlyList<string> Features,
    EntitlementQuotas Quotas,
    EntitlementPlanMetadata? PlanMetadata = null)
{
    /// <summary>Feature code for analytics.</summary>
    public const string FeatureAnalytics = "Analytics";

    /// <summary>Feature code for support.</summary>
    public const string FeatureSupport = "Support";

    /// <summary>Feature code for advanced security.</summary>
    public const string FeatureAdvancedSecurity = "AdvancedSecurity";

    /// <summary>Feature code for custom domain.</summary>
    public const string FeatureCustomDomain = "CustomDomain";

    /// <summary>Returns whether the given feature is included in this snapshot.</summary>
    public bool HasFeature(string featureCode)
    {
        if (string.IsNullOrWhiteSpace(featureCode)) return false;
        return Features.Contains(featureCode, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>Gets a quota value by key. Returns null if not defined. Use EntitlementQuotas.Unlimited for unlimited.</summary>
    public int? GetQuota(string key)
    {
        return key switch
        {
            nameof(EntitlementQuotas.MaxUsers) => Quotas.MaxUsers,
            nameof(EntitlementQuotas.MaxApplications) => Quotas.MaxApplications,
            nameof(EntitlementQuotas.MaxStorageMb) => Quotas.MaxStorageMb,
            nameof(EntitlementQuotas.MaxMembers) => Quotas.MaxMembers,
            nameof(EntitlementQuotas.MaxApiCalls) => Quotas.MaxApiCalls,
            _ => null
        };
    }
}
