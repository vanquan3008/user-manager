namespace UserService.Application.Exceptions;

public sealed class VerificationEmailResentException : Exception
{
    public VerificationEmailResentException(string message) : base(message) { }
}

