namespace UserService.Domain.Exceptions;

public sealed class BusinessRuleValidationException : DomainException
{
    public string RuleName { get; }

    public BusinessRuleValidationException(string ruleName, string message)
        : base($"Business rule '{ruleName}' was violated: {message}")
    {
        RuleName = ruleName;
    }

    public BusinessRuleValidationException(string message)
        : base(message)
    {
        RuleName = "Unknown";
    }
}
