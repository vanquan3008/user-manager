using UserService.Domain.Exceptions;

namespace UserService.Domain.ValueObjects;

public sealed class PersonName : ValueObject
{
    public string FirstName { get; }
    public string LastName { get; }
    public string DisplayName { get; }

    private PersonName(string firstName, string lastName, string displayName)
    {
        FirstName = firstName;
        LastName = lastName;
        DisplayName = displayName;
    }

    public static PersonName Create(string firstName, string lastName, string? displayName = null)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new InvalidDomainObjectException(nameof(FirstName), firstName, "First name cannot be empty.");

        //if (string.IsNullOrWhiteSpace(lastName))
        //    throw new InvalidDomainObjectException(nameof(LastName), lastName, "Last name cannot be empty.");

        var cleanFirstName = firstName.Trim();
        var cleanLastName = lastName.Trim();
        var cleanDisplayName = string.IsNullOrWhiteSpace(displayName)
            ? $"{cleanFirstName} {cleanLastName}"
            : displayName.Trim();

        return new PersonName(cleanFirstName, cleanLastName, cleanDisplayName);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return FirstName.ToLowerInvariant();
        yield return LastName.ToLowerInvariant();
        yield return DisplayName.ToLowerInvariant();
    }

    public override string ToString() => DisplayName;
}
