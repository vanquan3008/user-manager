namespace UserService.Domain.ValueObjects;

public sealed record PlanDescription
{
    public string Value { get; }

    private PlanDescription(string value) => Value = value;

    public static PlanDescription Create(string value)
    {
        if (value != null && value.Length > 1000)
            throw new ArgumentException("Description cannot exceed 1000 characters.", nameof(value));

        return new PlanDescription(value?.Trim() ?? string.Empty);
    }
    
    public static implicit operator string(PlanDescription desc) => desc.Value;
}
