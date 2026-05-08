namespace UserService.Domain.Exceptions;

public sealed class AuthenticationException : DomainException
{
    public string Reason { get; }

    public AuthenticationException(string reason)
        : base($"Authentication failed: {reason}")
    {
        Reason = reason;
    }
}
