namespace UserService.Domain.Entities;

public sealed class ApplicationRole
{
    public Guid ApplicationId { get; private set; }
    public Guid RoleId { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private ApplicationRole() { }

    public ApplicationRole(Guid applicationId, Guid roleId)
    {
        ApplicationId = applicationId;
        RoleId = roleId;
        CreatedAt = DateTime.UtcNow;
    }
}
