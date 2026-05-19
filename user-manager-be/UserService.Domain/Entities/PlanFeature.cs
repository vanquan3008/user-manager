namespace UserService.Domain.Entities;

/// <summary>
/// Feature entitlement for a subscription plan. Optional LimitsJson is for feature-scoped quotas.
/// </summary>
public sealed class PlanFeature
{
    private PlanFeature() { }

    public PlanFeature(Guid subscriptionPlanId, Guid featureId, bool isEnabled = true, string? limitsJson = null)
    {
        SubscriptionPlanId = subscriptionPlanId;
        FeatureId = featureId;
        IsEnabled = isEnabled;
        LimitsJson = string.IsNullOrWhiteSpace(limitsJson) ? null : limitsJson.Trim();
    }

    public Guid SubscriptionPlanId { get; private set; }
    public Guid FeatureId { get; private set; }
    public bool IsEnabled { get; private set; }
    public string? LimitsJson { get; private set; }

    public SubscriptionPlan SubscriptionPlan { get; private set; } = null!;
    public Feature Feature { get; private set; } = null!;

    public void Enable(string? limitsJson = null)
    {
        IsEnabled = true;
        LimitsJson = string.IsNullOrWhiteSpace(limitsJson) ? null : limitsJson.Trim();
    }

    public void Disable()
    {
        IsEnabled = false;
    }
}
