namespace UserService.Domain.Entities;

public abstract class Entity<TId> : IAuditable
{
    public TId Id { get; protected set; } = default!;
    public DateTime CreatedAt { get; protected set; }
    public DateTime? UpdatedAt { get; protected set; }
    protected Entity() { }
    protected virtual void MarkCreated()
    {
    }
    protected virtual void Touch()
    {
    }

    void IAuditable.SetCreatedAt(DateTime utcNow)
    {
        CreatedAt = utcNow;
        MarkCreated();
    }

    void IAuditable.SetUpdatedAt(DateTime utcNow)
    {
        UpdatedAt = utcNow;
        Touch();
    }
}
