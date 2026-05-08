namespace UserService.Domain.Exceptions;


public sealed class UserAlreadyInApplicationException : DomainException
{
    public Guid UserId { get; }
    public Guid ApplicationId { get; }

    public UserAlreadyInApplicationException(Guid userId, Guid applicationId)
        : base($"User '{userId}' is already a member of application '{applicationId}'.")
    {
        UserId = userId;
        ApplicationId = applicationId;
    }
}
