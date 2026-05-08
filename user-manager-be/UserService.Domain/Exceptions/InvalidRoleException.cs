namespace UserService.Domain.Exceptions;

public class InvalidRoleException : DomainException
{
    public string Role { get; }

    public InvalidRoleException(string role) 
        : base($"Role '{role}' is not supported or invalid")
    {
        Role = role;
    }
}
