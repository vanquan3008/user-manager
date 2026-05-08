namespace UserService.Domain.ValueObjects;

public sealed record UrlValue
{
    public string Value { get; }

    private UrlValue(string value) => Value = value;

    public static UrlValue Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("URL cannot be empty.", nameof(value));

        if (!Uri.TryCreate(value, UriKind.Absolute, out var uriResult) 
            || (uriResult.Scheme != Uri.UriSchemeHttp && uriResult.Scheme != Uri.UriSchemeHttps))
        {
            throw new ArgumentException("Invalid URL format. Must be an absolute HTTP/HTTPS URL.", nameof(value));
        }

        return new UrlValue(value.Trim());
    }
    
    public static implicit operator string(UrlValue url) => url.Value;
}
