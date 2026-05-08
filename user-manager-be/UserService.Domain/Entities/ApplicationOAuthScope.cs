namespace UserService.Domain.Entities;

public sealed class ApplicationOAuthScope : Entity<Guid>
{
    private ApplicationOAuthScope() : base() { }

    public ApplicationOAuthScope(Guid id, Guid applicationId, Guid permissionId)
        : base()
    {
        Id = id;
        ApplicationId = applicationId;
        PermissionId = permissionId;
        IsActive = true;
        MarkCreated();
    }

    public Guid ApplicationId { get; private set; }
    public Guid PermissionId { get; private set; }
    public bool IsActive { get; private set; }
    public AppApplication Application { get; private set; } = null!;
    public Permission Permission { get; private set; } = null!;
    public void Activate()
    {
        IsActive = true;
        Touch();
    }
    public void Deactivate()
    {
        IsActive = false;
        Touch();
    }
}

