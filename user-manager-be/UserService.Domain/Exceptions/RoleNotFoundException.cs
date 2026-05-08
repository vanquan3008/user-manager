namespace UserService.Domain.Exceptions;

public sealed class RoleNotFoundException : DomainException
{
    public string RoleName { get; }
    public Guid ApplicationId { get; }

    public RoleNotFoundException(string roleName, Guid applicationId)
        : base($"Role '{roleName}' does not exist for application '{applicationId}'.")
    {
        RoleName = roleName;
        ApplicationId = applicationId;
    }
}
