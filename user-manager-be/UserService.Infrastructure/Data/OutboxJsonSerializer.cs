using System.Text.Json;
using UserService.Domain.Events;

namespace UserService.Infrastructure.Data;

internal static class OutboxJsonSerializer
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public static string Serialize(IDomainEvent domainEvent)
    {
        return JsonSerializer.Serialize(domainEvent, domainEvent.GetType(), SerializerOptions);
    }

    public static IDomainEvent Deserialize(string payload, string typeName)
    {
        var type = Type.GetType(typeName, throwOnError: false);
        if (type == null)
            throw new InvalidOperationException($"Unable to resolve outbox event type '{typeName}'.");

        var deserialized = JsonSerializer.Deserialize(payload, type, SerializerOptions);
        if (deserialized is not IDomainEvent domainEvent)
            throw new InvalidOperationException($"Unable to deserialize outbox payload to '{typeName}'.");

        return domainEvent;
    }
}
