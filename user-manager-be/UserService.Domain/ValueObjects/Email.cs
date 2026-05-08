using UserService.Domain.Exceptions;

namespace UserService.Domain.ValueObjects;

public sealed class Email : ValueObject
{
    public string Value { get; }

    private Email(string value)
    {
        Value = value;
    }

    public static Email Create(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new InvalidDomainObjectException(nameof(Email), email, "Email cannot be empty");
        }

        email = email.Trim().ToLowerInvariant();

        if (!IsValidEmail(email))
        {
            throw new InvalidDomainObjectException(nameof(Email), email, "Invalid email format");
        }

        if (email.Length > 256)
        {
            throw new InvalidDomainObjectException(nameof(Email), email, "Email cannot exceed 256 characters");
        }

        return new Email(email);
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    public static implicit operator string(Email email) => email.Value;
}
