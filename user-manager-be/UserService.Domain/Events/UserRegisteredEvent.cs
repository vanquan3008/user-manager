namespace UserService.Domain.Events;

public sealed record UserRegisteredEvent(
    Guid UserId,
    string Email,
    string Username
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
