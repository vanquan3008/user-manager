namespace UserService.Domain.ValueObjects;

public sealed class Address : ValueObject
{
    public string? Country { get; }
    public string? City { get; }
    public string? State { get; }
    public string? StreetAddress { get; }
    public string? PostalCode { get; }

    private Address(string? country, string? city, string? state, string? streetAddress, string? postalCode)
    {
        Country = country;
        City = city;
        State = state;
        StreetAddress = streetAddress;
        PostalCode = postalCode;
    }

    public static Address Create(string? country = null, string? city = null, string? state = null, string? streetAddress = null, string? postalCode = null)
    {
        return new Address(
            country?.Trim(),
            city?.Trim(),
            state?.Trim(),
            streetAddress?.Trim(),
            postalCode?.Trim()
        );
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Country?.ToLowerInvariant();
        yield return City?.ToLowerInvariant();
        yield return State?.ToLowerInvariant();
        yield return StreetAddress?.ToLowerInvariant();
        yield return PostalCode?.ToLowerInvariant();
    }

    public override string ToString()
    {
        var parts = new[] { StreetAddress, City, State, PostalCode, Country }
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .Select(p => p!.Trim());
        return string.Join(", ", parts);
    }
}
