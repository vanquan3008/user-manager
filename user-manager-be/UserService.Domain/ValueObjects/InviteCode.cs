namespace UserService.Domain.ValueObjects;

public sealed record InviteCode
{
    public string Value { get; }

    private InviteCode(string value) => Value = value;

    public static InviteCode Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Invite Code cannot be empty.", nameof(value));

        return new InviteCode(value.Trim().ToUpperInvariant());
    }

    public static InviteCode Generate()
    {
        return new InviteCode(Guid.NewGuid().ToString("N").Substring(0, 12).ToUpperInvariant());
    }

    public static implicit operator string(InviteCode code) => code.Value;
}
