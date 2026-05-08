namespace UserService.Domain.ValueObjects;

public sealed record TokenValue
{
    public string Value { get; }

    private TokenValue(string value) => Value = value;

    public static TokenValue Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Token cannot be empty.", nameof(value));

        return new TokenValue(value);
    }

    public static implicit operator string(TokenValue token) => token.Value;
}
