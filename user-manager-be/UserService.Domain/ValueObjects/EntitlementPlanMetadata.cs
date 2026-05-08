namespace UserService.Domain.ValueObjects;
public sealed record EntitlementPlanMetadata(
    Guid PlanId,
    string PlanName,
    decimal? MonthlyPrice = null,
    decimal? YearlyPrice = null);
