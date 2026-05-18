using UserService.Domain.ValueObjects;

namespace UserService.Domain.Entities;

public sealed class SubscriptionPlan : AuditableAggregateRoot<Guid>
{
    private readonly List<PlanFeature> _features = new();
    private readonly List<SubscriptionPlanPermission> _permissions = new();

    private SubscriptionPlan() : base() { }

    private SubscriptionPlan(
        Guid id,
        Guid? applicationId,
        PlanName name,
        PlanDescription description,
        Money monthlyPrice,
        Money yearlyPrice,
        int maxUsers)
        : base()
    {
        Id = id;
        ApplicationId = applicationId;
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Description = description ?? throw new ArgumentNullException(nameof(description));
        MonthlyPrice = monthlyPrice ?? throw new ArgumentNullException(nameof(monthlyPrice));
        YearlyPrice = yearlyPrice ?? throw new ArgumentNullException(nameof(yearlyPrice));
        MaxUsers = maxUsers;
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
        int maxUsers = -1)
    {
        return new SubscriptionPlan(
            id, applicationId, name, description, monthlyPrice, yearlyPrice,
            maxUsers);
    }

    public Guid? ApplicationId { get; private set; }

    public PlanName Name { get; private set; } = null!;
    public PlanDescription Description { get; private set; } = null!;
    public UrlValue? Icon { get; private set; }
    
    public Money MonthlyPrice { get; private set; } = null!;
    public Money YearlyPrice { get; private set; } = null!;
    
    public int MaxUsers { get; private set; } = -1;
    public int? MaxApiCalls { get; private set; }
    
    public bool IsActive { get; private set; }
    public int DisplayOrder { get; private set; }
    public IReadOnlyCollection<PlanFeature> Features => _features.AsReadOnly();
    public IReadOnlyCollection<SubscriptionPlanPermission> Permissions => _permissions.AsReadOnly();

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

    public void UpdateLimits(int maxUsers, int? maxApiCalls = null)
    {
        MaxUsers = maxUsers;
        // -1 hoặc null đều hiểu là không giới hạn (lưu null vào DB theo constraint)
        MaxApiCalls = maxApiCalls == -1 ? null : maxApiCalls;
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

    public void ReplaceFeatureAssignments(IEnumerable<Guid> featureIds)
    {
        var targetIds = NormalizeIds(featureIds);
        _features.RemoveAll(item => !targetIds.Contains(item.FeatureId));

        var existingIds = _features
            .Select(item => item.FeatureId)
            .ToHashSet();

        foreach (var featureId in targetIds.Where(id => !existingIds.Contains(id)))
        {
            _features.Add(new PlanFeature(Id, featureId));
        }

        Touch();
    }

    public void ReplacePermissionAssignments(IEnumerable<Guid> permissionIds)
    {
        var targetIds = NormalizeIds(permissionIds);
        _permissions.RemoveAll(item => !targetIds.Contains(item.PermissionId));

        var existingIds = _permissions
            .Select(item => item.PermissionId)
            .ToHashSet();

        foreach (var permissionId in targetIds.Where(id => !existingIds.Contains(id)))
        {
            _permissions.Add(new SubscriptionPlanPermission(Id, permissionId));
        }

        Touch();
    }
    
    public EntitlementSnapshot CreateEntitlementSnapshot(
        IReadOnlyCollection<string> featureCodes,
        int maxMembersForAssignment = 1)
    {
        var quotas = new EntitlementQuotas(
            MaxUsers,
            EntitlementQuotas.Unlimited,
            EntitlementQuotas.Unlimited,
            maxMembersForAssignment,
            MaxApiCalls);

        var metadata = new EntitlementPlanMetadata(Id, Name, MonthlyPrice.Amount, YearlyPrice.Amount);
        return new EntitlementSnapshot(featureCodes.ToList(), quotas, metadata);
    }

    private static HashSet<Guid> NormalizeIds(IEnumerable<Guid> ids)
    {
        return ids
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToHashSet();
    }
}
