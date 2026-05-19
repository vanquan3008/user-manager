namespace UserService.Domain.Exceptions;

public sealed class ApplicationInactiveException : DomainException
{
    public Guid? ApplicationId { get; }

    public ApplicationInactiveException(Guid? applicationId = null)
        : base(applicationId.HasValue
            ? $"Application '{applicationId}' is not found or inactive."
            : "Application is not found or inactive.")
    {
        ApplicationId = applicationId;
    }
}
