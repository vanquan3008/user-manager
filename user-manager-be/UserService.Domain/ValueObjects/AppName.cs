namespace UserService.Domain.ValueObjects;

public sealed record AppName
{
    public string Value { get; }

    private AppName(string value) => Value = value;

    public static AppName Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("App name cannot be empty.", nameof(value));
        
        if (value.Length > 100)
            throw new ArgumentException("App name cannot exceed 100 characters.", nameof(value));

        return new AppName(value.Trim());
    }
    
    public static implicit operator string(AppName name) => name.Value;
}
