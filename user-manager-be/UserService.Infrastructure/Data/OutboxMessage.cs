using UserService.Domain.Events;

namespace UserService.Infrastructure.Data;

public sealed class OutboxMessage
{
    private OutboxMessage()
    {
    }

    private OutboxMessage(Guid id, DateTime occurredOnUtc, string type, string payload)
    {
        Id = id;
        OccurredOnUtc = occurredOnUtc;
        Type = type;
        Payload = payload;
    }

    public Guid Id { get; private set; }
    public DateTime OccurredOnUtc { get; private set; }
    public string Type { get; private set; } = string.Empty;
    public string Payload { get; private set; } = string.Empty;
    public DateTime? ProcessedOnUtc { get; private set; }
    public DateTime? LastAttemptedOnUtc { get; private set; }
    public int AttemptCount { get; private set; }
    public string? Error { get; private set; }

    public bool IsProcessed => ProcessedOnUtc.HasValue;

    public static OutboxMessage Create(IDomainEvent domainEvent, string payload)
    {
        var type = domainEvent.GetType().AssemblyQualifiedName
            ?? throw new InvalidOperationException("Domain event type must have an assembly-qualified name.");

        return new OutboxMessage(
            Guid.NewGuid(),
            domainEvent.OccurredOn,
            type,
            payload);
    }

    public void MarkProcessed(DateTime processedOnUtc)
    {
        ProcessedOnUtc = processedOnUtc;
        LastAttemptedOnUtc = processedOnUtc;
        AttemptCount += 1;
        Error = null;
    }

    public void MarkFailed(DateTime attemptedOnUtc, string error)
    {
        LastAttemptedOnUtc = attemptedOnUtc;
        AttemptCount += 1;
        Error = string.IsNullOrWhiteSpace(error) ? "Unknown outbox processing error." : error.Trim();
    }
}
