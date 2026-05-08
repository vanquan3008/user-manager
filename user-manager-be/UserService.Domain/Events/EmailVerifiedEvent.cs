namespace UserService.Domain.Events;

public sealed record EmailVerifiedEvent(
    Guid UserId,
    string Email
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
