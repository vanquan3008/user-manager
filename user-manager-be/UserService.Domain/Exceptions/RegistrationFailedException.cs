namespace UserService.Domain.Exceptions;

public sealed class RegistrationFailedException : DomainException
{
    public IEnumerable<string> Errors { get; }

    public RegistrationFailedException(IEnumerable<string> errors)
        : base($"Registration failed: {string.Join(", ", errors)}")
    {
        Errors = errors;
    }

    public RegistrationFailedException(string message)
        : base($"Registration failed: {message}")
    {
        Errors = new[] { message };
    }
}
