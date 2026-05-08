using UserService.Domain.Events;

namespace UserService.Domain.Entities;

public abstract class AggregateRoot<TId> : Entity<TId>, IAggregateRoot, ISoftDelete
{
    private readonly List<IDomainEvent> _domainEvents = [];
    public DateTime? DeletedAt { get; protected set; }
    public bool IsDeleted { get; protected set; }
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();
    protected AggregateRoot() : base() { }
    protected void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    protected void MarkDeleted()
    {
        if (IsDeleted) return;
        IsDeleted = true;
        Touch();
    }
    public void Restore()
    {
        if (!IsDeleted) return;
        IsDeleted = false;
        DeletedAt = null;
        Touch();
    }

    void ISoftDelete.SoftDelete(DateTime utcNow)
    {
        if (IsDeleted)
        {
            DeletedAt ??= utcNow;
            return;
        }

        IsDeleted = true;
        DeletedAt = utcNow;
        Touch();
    }

    void ISoftDelete.RestoreSoftDelete()
    {
        if (!IsDeleted) return;
        IsDeleted = false;
        DeletedAt = null;
        Touch();
    }
}
