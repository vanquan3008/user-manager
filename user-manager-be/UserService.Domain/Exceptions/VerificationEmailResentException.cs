namespace UserService.Domain.Exceptions;

public sealed class VerificationEmailResentException : DomainException
{
    public VerificationEmailResentException(string message)
        : base(message)
    {
    }
}
