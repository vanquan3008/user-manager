namespace UserService.Domain.ValueObjects;

public sealed record PlanName
{
    public string Value { get; }

    private PlanName(string value) => Value = value;

    public static PlanName Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Plan name cannot be empty.", nameof(value));
        
        if (value.Length > 150)
            throw new ArgumentException("Plan name cannot exceed 150 characters.", nameof(value));

        return new PlanName(value.Trim());
    }
    
    public static implicit operator string(PlanName name) => name.Value;
}
