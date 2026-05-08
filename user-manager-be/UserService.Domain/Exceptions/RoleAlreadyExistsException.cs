namespace UserService.Domain.Exceptions;

public sealed class RoleAlreadyExistsException : DomainException
{
    public string RoleName { get; }
    public Guid ApplicationId { get; }

    public RoleAlreadyExistsException(string roleName, Guid applicationId)
        : base($"Role '{roleName}' already exists for application '{applicationId}'.")
    {
        RoleName = roleName;
        ApplicationId = applicationId;
    }
}
