namespace UserService.Domain.Exceptions;

/// <summary>
/// Exception khi validation th?t b?i
/// </summary>
public sealed class InvalidDomainObjectException : DomainException
{
    public string PropertyName { get; }
    public object? AttemptedValue { get; }

    public InvalidDomainObjectException(string propertyName, object? attemptedValue, string message)
        : base($"Invalid value for {propertyName}: {message}")
    {
        PropertyName = propertyName;
        AttemptedValue = attemptedValue;
    }

    public InvalidDomainObjectException(string message)
        : base(message)
    {
        PropertyName = "Unknown";
    }
}
