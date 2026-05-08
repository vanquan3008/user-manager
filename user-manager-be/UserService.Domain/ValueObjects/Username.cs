using UserService.Domain.Exceptions;
using System.Text.RegularExpressions;

namespace UserService.Domain.ValueObjects;

/// <summary>
/// Value Object cho Username
/// </summary>
public sealed class Username : ValueObject
{
    private static readonly Regex UsernameRegex = new(@"^[a-zA-Z0-9_]{3,30}$", RegexOptions.Compiled);

    public string Value { get; }

    private Username(string value)
    {
        Value = value;
    }

    public static Username Create(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            throw new InvalidDomainObjectException(nameof(Username), username, "Username cannot be empty");
        }

        username = username.Trim();

        if (!UsernameRegex.IsMatch(username))
        {
            throw new InvalidDomainObjectException(nameof(Username), username,
                "Username must be 3-30 characters and contain only letters, numbers, and underscores");
        }

        return new Username(username);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value.ToLowerInvariant();
    }

    public override string ToString() => Value;

    public static implicit operator string(Username username) => username.Value;
}
