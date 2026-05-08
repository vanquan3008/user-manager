namespace UserService.Domain.Events;

public interface IDomainEvent
{   
    DateTime OccurredOn { get; }
}
