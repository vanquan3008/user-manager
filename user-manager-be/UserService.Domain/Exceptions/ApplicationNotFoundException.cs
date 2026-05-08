namespace UserService.Domain.Exceptions;
public sealed class ApplicationNotFoundException : DomainException
{
    public Guid ApplicationId { get; }

    public ApplicationNotFoundException(Guid applicationId)
        : base($"Application with ID '{applicationId}' was not found.")
    {
        ApplicationId = applicationId;
    }
}
