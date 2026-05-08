using UserService.Domain.ValueObjects;

namespace UserService.Domain.Entities;

public sealed class Permission : AggregateRoot<Guid>
{
    private Permission() { }

    private Permission(
        PermissionCode code,
        string description,
        string category,
        bool isSystemPermission = false)
    {
        Id = Guid.NewGuid();
        Code = code ?? throw new ArgumentNullException(nameof(code));
        Description = description?.Trim() ?? throw new ArgumentNullException(nameof(description));
        Category = category?.Trim() ?? throw new ArgumentNullException(nameof(category));
        IsSystemPermission = isSystemPermission;
        MarkCreated();
    }
    
    public static Permission Create(
        PermissionCode code,
        string description,
        string category,
        bool isSystemPermission = false)
    {
        return new Permission(code, description, category, isSystemPermission);
    }

    public PermissionCode Code { get; private set; } = null!;
    public string Description { get; private set; } = string.Empty;
    public string Category { get; private set; } = string.Empty;

    public bool IsSystemPermission { get; private set; }
    public bool IsActive { get; private set; } = true;
    public string? Scope { get; private set; }

    public ICollection<RolePermission> RolePermissions { get; private set; } = new List<RolePermission>();

    public void UpdateDescription(string newDescription)
    {
        Description = newDescription?.Trim() ?? throw new ArgumentNullException(nameof(newDescription));
        Touch();
    }

    public void SetActive(bool isActive)
    {
        IsActive = isActive;
        Touch();
    }

    public void SetScope(string? scope)
    {
        Scope = scope;
        Touch();
    }
}
