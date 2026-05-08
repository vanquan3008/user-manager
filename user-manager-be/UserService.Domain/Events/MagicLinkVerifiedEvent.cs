namespace UserService.Domain.Events;

public sealed record MagicLinkVerifiedEvent(
    Guid UserId,
    string Purpose
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
