using UserService.Domain.Enums;

namespace UserService.Domain.Events;

public record SubscriptionActivatedEvent(Guid SubscriptionId, Guid UserId, SubscriptionType Type) : IDomainEvent
{
    public DateTime OccurredOn => DateTime.UtcNow;
}

public record SubscriptionUpgradedEvent(Guid SubscriptionId, Guid UserId, SubscriptionType NewType, DateTime? ExpiresAt) : IDomainEvent
{
    public DateTime OccurredOn => DateTime.UtcNow;
}

public record SubscriptionExtendedEvent(Guid SubscriptionId, Guid UserId, DateTime NewExpiresAt) : IDomainEvent
{
    public DateTime OccurredOn => DateTime.UtcNow;
}

public record SubscriptionCancelledEvent(Guid SubscriptionId, Guid UserId) : IDomainEvent
{
    public DateTime OccurredOn => DateTime.UtcNow;
}

public record SubscriptionExpiredEvent(Guid SubscriptionId, Guid UserId) : IDomainEvent
{
    public DateTime OccurredOn => DateTime.UtcNow;
}
