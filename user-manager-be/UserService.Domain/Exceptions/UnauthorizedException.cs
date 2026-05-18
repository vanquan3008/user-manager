namespace UserService.Domain.Exceptions;

public sealed class UnauthorizedException : DomainException
{
    public string Reason { get; }

    public UnauthorizedException(string reason)
        : base($"Unauthorized: {reason}")
    {
        Reason = reason;
    }
}
