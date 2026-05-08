namespace UserService.Domain.Exceptions;

public sealed class DomainOperationNotAllowedException : DomainException
{
    public string Operation { get; }
    public string Reason { get; }

    public DomainOperationNotAllowedException(string operation, string reason)
        : base($"Operation '{operation}' is not allowed: {reason}")
    {
        Operation = operation;
        Reason = reason;
    }
}
