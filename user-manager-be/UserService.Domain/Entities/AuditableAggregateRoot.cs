namespace UserService.Domain.Entities;

public abstract class AuditableAggregateRoot<TId> : AggregateRoot<TId>
{
    protected AuditableAggregateRoot() : base() { }
    public Guid? CreatedBy { get; protected set; }
    public Guid? UpdatedBy { get; protected set; }
    public Guid? DeletedBy { get; protected set; }
    protected void MarkCreated(Guid? userId = null)
    {
        base.MarkCreated();
        CreatedBy = userId;
    }

    protected void MarkUpdated(Guid? userId = null)
    {
        base.Touch();
        UpdatedBy = userId;
    }

    protected override void Touch() => MarkUpdated(null);
    protected void MarkDeleted(Guid? userId = null)
    {
        base.MarkDeleted();
        DeletedBy = userId;
        UpdatedBy = userId;
    }

    public void RestoreWithAudit(Guid? userId = null)
    {
        base.Restore();
        DeletedBy = null;
        UpdatedBy = userId;
    }
}

