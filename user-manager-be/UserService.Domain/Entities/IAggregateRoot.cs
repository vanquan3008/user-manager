using UserService.Domain.Events;

namespace UserService.Domain.Entities;
public interface IAggregateRoot
{
    IReadOnlyCollection<IDomainEvent> DomainEvents { get; }
    void ClearDomainEvents();
    bool IsDeleted { get; }
    DateTime? DeletedAt { get; }
}
