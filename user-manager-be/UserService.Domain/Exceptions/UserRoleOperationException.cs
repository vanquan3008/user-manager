namespace UserService.Domain.Exceptions;

/// <summary>
/// Thrown when a role operation (add/remove) fails
/// </summary>
public class UserRoleOperationException : DomainException
{
    public Guid UserId { get; }
    public string Role { get; }
    public string Operation { get; }

    public UserRoleOperationException(Guid userId, string role, string operation, string? details = null) 
        : base($"Failed to {operation} role '{role}' for user '{userId}'. {details}")
    {
        UserId = userId;
        Role = role;
        Operation = operation;
    }
}
