namespace UserService.Domain.Exceptions;

/// <summary>
/// Thrown when a user is not found
/// </summary>
public class UserNotFoundException : DomainException
{
    public Guid UserId { get; }

    public UserNotFoundException(Guid userId) 
        : base($"User with ID '{userId}' was not found")
    {
        UserId = userId;
    }
}
