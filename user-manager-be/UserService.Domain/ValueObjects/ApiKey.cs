namespace UserService.Domain.ValueObjects;

public sealed record ApiKey
{
    public string Value { get; }

    private ApiKey(string value) => Value = value;

    public static ApiKey Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("API Key cannot be empty.", nameof(value));
            
        return new ApiKey(value);
    }

    public static ApiKey Generate()
    {
        return new ApiKey($"app_{Guid.NewGuid().ToString("N").Substring(0, 20)}");
    }
    
    public static implicit operator string(ApiKey key) => key.Value;
}
