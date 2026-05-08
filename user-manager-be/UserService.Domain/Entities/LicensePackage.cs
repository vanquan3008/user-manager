using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using UserService.Domain.ValueObjects;

namespace UserService.Domain.Entities;

public sealed class LicensePackage : AuditableAggregateRoot<Guid>
{
    private readonly List<Subscription> _subscriptions = new();

    private LicensePackage() : base() { }

    private LicensePackage(
        Guid id,
        Guid applicationId,
        Guid subscriptionPlanId,
        PlanName name,
        string featureJson,
        string assignedRole,
        int seatLimit = -1)
        : base()
    {
        Id = id;
        ApplicationId = applicationId;
        SubscriptionPlanId = subscriptionPlanId;
        Name = name ?? throw new ArgumentNullException(nameof(name));
        FeatureJson = featureJson ?? "{}";
        AssignedRole = assignedRole;
        SeatLimit = seatLimit;
        IsActive = true;
        MarkCreated();
    }

    public static LicensePackage Create(
        Guid id,
        Guid applicationId,
        Guid subscriptionPlanId,
        PlanName name,
        string featureJson,
        string assignedRole,
        int seatLimit = -1)
    {
        return new LicensePackage(id, applicationId, subscriptionPlanId, name, featureJson, assignedRole, seatLimit);
    }

    public Guid ApplicationId { get; private set; }
    public Guid? SubscriptionPlanId { get; private set; }
    
    public PlanName Name { get; private set; } = null!;
    public string FeatureJson { get; private set; } = "{}";
    public int SeatLimit { get; private set; } = -1;

    public string AssignedRole { get; private set; } = string.Empty;

    public bool IsActive { get; private set; }
    public int DisplayOrder { get; private set; }

    [ForeignKey(nameof(ApplicationId))]
    public AppApplication Application { get; private set; } = null!;

    [ForeignKey(nameof(SubscriptionPlanId))]
    public SubscriptionPlan? SubscriptionPlan { get; private set; }

    public IReadOnlyCollection<Subscription> Subscriptions => _subscriptions.AsReadOnly();

    public EntitlementSnapshot CreateEntitlementSnapshot()
    {
        var features = GetFeatures();
        var quotas = new EntitlementQuotas(
            EntitlementQuotas.Unlimited,
            EntitlementQuotas.Unlimited,
            EntitlementQuotas.Unlimited,
            SeatLimit < 0 ? EntitlementQuotas.Unlimited : SeatLimit,
            null);
        var metadata = new EntitlementPlanMetadata(Id, Name, null, null);
        return new EntitlementSnapshot(features, quotas, metadata);
    }

    public IReadOnlyList<string> GetFeatures() => ParseFeaturesFromJson(FeatureJson);

    private static IReadOnlyList<string> ParseFeaturesFromJson(string featureJson)
    {
        if (string.IsNullOrWhiteSpace(featureJson) || featureJson.Trim() == "{}")
            return Array.Empty<string>();
        try
        {
            var list = JsonSerializer.Deserialize<string[]>(featureJson);
            return list ?? Array.Empty<string>();
        }
        catch
        {
            return Array.Empty<string>();
        }
    }

    public void Update(PlanName name, string featureJson, int seatLimit)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        FeatureJson = featureJson ?? "{}";
        SeatLimit = seatLimit;
        Touch();
    }

    public void SetAssignedRole(string assignedRole)
    {
        AssignedRole = assignedRole;
        Touch();
    }

    public void Update(PlanName name, string featureJson, int seatLimit, string assignedRole)
    {
        Update(name, featureJson, seatLimit);
        SetAssignedRole(assignedRole);
    }

    public void SetDisplayOrder(int order)
    {
        DisplayOrder = order;
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
}
