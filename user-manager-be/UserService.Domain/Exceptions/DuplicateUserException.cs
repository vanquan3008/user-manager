namespace UserService.Domain.Exceptions;

public sealed class DuplicateUserException : DomainException
{
    public string DuplicateField { get; }
    public string DuplicateValue { get; }

    public DuplicateUserException(string duplicateField, string duplicateValue)
        : base($"User with {duplicateField} '{duplicateValue}' already exists.")
    {
        DuplicateField = duplicateField;
        DuplicateValue = duplicateValue;
    }
}
