namespace UserService.Domain.ValueObjects;

public sealed record AppDescription
{
    public string Value { get; }

    private AppDescription(string value) => Value = value;

    public static AppDescription Create(string value)
    {
        if (value != null && value.Length > 500)
            throw new ArgumentException("Description cannot exceed 500 characters.", nameof(value));

        return new AppDescription(value?.Trim() ?? string.Empty);
    }
    
    public static implicit operator string(AppDescription desc) => desc.Value;
}
