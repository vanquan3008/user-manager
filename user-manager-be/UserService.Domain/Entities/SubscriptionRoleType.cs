namespace UserService.Domain.Entities;

public sealed class SubscriptionRoleType
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty; 
    public int SortOrder { get; private set; }
    public bool IsAdmin { get; private set; } 

    private SubscriptionRoleType() { }

    internal SubscriptionRoleType(Guid id, string name, int sortOrder, bool isAdmin)
    {
        Id = id;
        Name = name ?? throw new ArgumentNullException(nameof(name));
        SortOrder = sortOrder;
        IsAdmin = isAdmin;
    }
}
