using UserService.Domain.Constants;
using UserService.Domain.ValueObjects;

namespace UserService.Domain.Entities;

public sealed class SubscriptionPlan : AuditableAggregateRoot<Guid>
{
    private SubscriptionPlan() : base() { }

    private SubscriptionPlan(
        Guid id,
        Guid? applicationId,
        PlanName name,
        PlanDescription description,
        Money monthlyPrice,
        Money yearlyPrice,
        int maxUsers,
        bool includeAnalytics,
        bool includeSupport)
        : base()
    {
        Id = id;
        ApplicationId = applicationId;
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Description = description ?? throw new ArgumentNullException(nameof(description));
        MonthlyPrice = monthlyPrice ?? throw new ArgumentNullException(nameof(monthlyPrice));
        YearlyPrice = yearlyPrice ?? throw new ArgumentNullException(nameof(yearlyPrice));
        MaxUsers = maxUsers;
        IncludeAnalytics = includeAnalytics;
        IncludeSupport = includeSupport;
        IsActive = true;
        
        MarkCreated();
    }

    public static SubscriptionPlan Create(
        Guid id,
        Guid? applicationId,
        PlanName name,
        PlanDescription description,
        Money monthlyPrice,
        Money yearlyPrice,
        int maxUsers = -1,
        bool includeAnalytics = false,
        bool includeSupport = false)
    {
        return new SubscriptionPlan(
            id, applicationId, name, description, monthlyPrice, yearlyPrice,
            maxUsers, includeAnalytics, includeSupport);
    }

    public Guid? ApplicationId { get; private set; }

    public PlanName Name { get; private set; } = null!;
    public PlanDescription Description { get; private set; } = null!;
    public UrlValue? Icon { get; private set; }
    
    public Money MonthlyPrice { get; private set; } = null!;
    public Money YearlyPrice { get; private set; } = null!;
    
    public int MaxUsers { get; private set; } = -1;
    public int? MaxApiCalls { get; private set; }
    
    public bool IncludeAnalytics { get; private set; }
    public bool IncludeSupport { get; private set; }
    public bool IncludeAdvancedSecurity { get; private set; }
    public bool IncludeCustomDomain { get; private set; }

    public List<SubscriptionPlanRoleAssignment> RoleAssignments { get; } = new();

    public bool IsActive { get; private set; }
    public int DisplayOrder { get; private set; }

    public void UpdatePlan(
        PlanName name,
        PlanDescription description,
        Money monthlyPrice,
        Money yearlyPrice)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Description = description ?? throw new ArgumentNullException(nameof(description));
        MonthlyPrice = monthlyPrice ?? throw new ArgumentNullException(nameof(monthlyPrice));
        YearlyPrice = yearlyPrice ?? throw new ArgumentNullException(nameof(yearlyPrice));
        Touch();
    }

    public void UpdateFeatures(
        bool includeAnalytics,
        bool includeSupport,
        bool includeAdvancedSecurity,
        bool includeCustomDomain)
    {
        IncludeAnalytics = includeAnalytics;
        IncludeSupport = includeSupport;
        IncludeAdvancedSecurity = includeAdvancedSecurity;
        IncludeCustomDomain = includeCustomDomain;
        Touch();
    }

    public void ReplaceRoleAssignments(IEnumerable<Guid> roleIds)
    {
        RoleAssignments.Clear();
        foreach (var rid in roleIds.Distinct())
            RoleAssignments.Add(new SubscriptionPlanRoleAssignment(Id, rid));
        Touch();
    }

    public string ToAssignedRolesCsv(IReadOnlyDictionary<Guid, string> roleNameById)
    {
        if (!ApplicationId.HasValue)
            return RoleNames.Guest;
        if (RoleAssignments.Count == 0)
            return RoleNames.Guest;
        var names = RoleAssignments
            .Select(a => roleNameById.GetValueOrDefault(a.RoleId))
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Select(s => s!.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(s => s, StringComparer.OrdinalIgnoreCase)
            .ToList();
        return names.Count == 0 ? RoleNames.Guest : AssignedRolesFormat.Normalize(string.Join(",", names));
    }

    public void UpdateLimits(int maxUsers, int? maxApiCalls = null)
    {
        MaxUsers = maxUsers;
        MaxApiCalls = maxApiCalls;
        Touch();
    }
    
    public void Activate()
    {
        IsActive = true;
        Touch();
    }
    
    public void Deactivate()
    {
        IsActive = false;
        Touch();
    }
    
    public void SetDisplayOrder(int order)
    {
        DisplayOrder = order;
        Touch();
    }
    
    public void SetIcon(UrlValue? icon)
    {
        Icon = icon;
        Touch();
    }
    
    public EntitlementSnapshot CreateEntitlementSnapshot(int maxMembersForAssignment = 1)
    {
        var features = new List<string>();
        if (IncludeAnalytics) features.Add(EntitlementSnapshot.FeatureAnalytics);
        if (IncludeSupport) features.Add(EntitlementSnapshot.FeatureSupport);
        if (IncludeAdvancedSecurity) features.Add(EntitlementSnapshot.FeatureAdvancedSecurity);
        if (IncludeCustomDomain) features.Add(EntitlementSnapshot.FeatureCustomDomain);

        var quotas = new EntitlementQuotas(
            MaxUsers,
            EntitlementQuotas.Unlimited,
            EntitlementQuotas.Unlimited,
            maxMembersForAssignment,
            MaxApiCalls);

        var metadata = new EntitlementPlanMetadata(Id, Name, MonthlyPrice.Amount, YearlyPrice.Amount);
        return new EntitlementSnapshot(features, quotas, metadata);
    }
}
