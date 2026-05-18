using System.Text.RegularExpressions;
using UserService.Domain.Exceptions;

namespace UserService.Domain.ValueObjects;

/// <summary>
/// Value Object representing a raw password value with policy validation.
/// This does NOT store a hash; it wraps and validates a plaintext password at the boundary.
/// </summary>
public sealed class Password : ValueObject
{
    public string Value { get; }

    private Password(string value)
    {
        Value = value;
    }

    public static Password Create(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidDomainObjectException(nameof(Password), password, "Password cannot be empty.");
        }

        if (password.Length < 8)
        {
            throw new InvalidDomainObjectException(nameof(Password), password, "Password must be at least 8 characters long.");
        }

        if (password.Length > 128)
        {
            throw new InvalidDomainObjectException(nameof(Password), password, "Password cannot exceed 128 characters.");
        }

        if (!Regex.IsMatch(password, @"[A-Z]"))
        {
            throw new InvalidDomainObjectException(nameof(Password), password, "Password must contain at least one uppercase letter.");
        }

        if (!Regex.IsMatch(password, @"[a-z]"))
        {
            throw new InvalidDomainObjectException(nameof(Password), password, "Password must contain at least one lowercase letter.");
        }

        if (!Regex.IsMatch(password, @"[0-9]"))
        {
            throw new InvalidDomainObjectException(nameof(Password), password, "Password must contain at least one digit.");
        }

        if (!Regex.IsMatch(password, @"[!@#$%^&*()_+\-=\[\]{}|;':"",./<>?]"))
        {
            throw new InvalidDomainObjectException(nameof(Password), password, "Password must contain at least one special character.");
        }

        return new Password(password);
    }

    /// <summary>
    /// Creates a Password without enforcing complexity rules. Use only for test seeds or migration scenarios.
    /// </summary>
    public static Password CreateUnsafe(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidDomainObjectException(nameof(Password), password, "Password cannot be empty.");
        }

        return new Password(password);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => "[REDACTED]";
}
