namespace UserService.Domain.Constants;

public static class ApplicationClientTypes
{
    public const string Public = "Public";
    public const string Confidential = "Confidential";

    public static bool IsValid(string? value)
        => string.Equals(value, Public, StringComparison.OrdinalIgnoreCase)
           || string.Equals(value, Confidential, StringComparison.OrdinalIgnoreCase);

    public static string Normalize(string? value)
    {
        if (string.Equals(value, Confidential, StringComparison.OrdinalIgnoreCase))
            return Confidential;

        return Public;
    }

    public static bool UsesClientSecret(string? value)
        => string.Equals(value, Confidential, StringComparison.OrdinalIgnoreCase);
}
