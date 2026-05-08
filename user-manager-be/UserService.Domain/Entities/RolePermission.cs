namespace UserService.Domain.Entities;

public sealed class RolePermission : Entity<Guid>
{
    private RolePermission()
    {
    }

    public RolePermission(Guid roleId, Guid permissionId)
        : base()
    {
        Id = Guid.NewGuid();
        RoleId = roleId;
        PermissionId = permissionId;
        MarkCreated();
    }

    public Guid RoleId { get; private set; }
    public Guid PermissionId { get; private set; }
    public Permission? Permission { get; private set; }
}
