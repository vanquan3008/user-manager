using System.Text.Json;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using UserService.Domain.ValueObjects;

namespace UserService.Infrastructure.Data;

public sealed class EntitlementSnapshotConverter : ValueConverter<EntitlementSnapshot?, string?>
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public EntitlementSnapshotConverter()
        : base(
            v => v == null ? null : JsonSerializer.Serialize(v, SerializerOptions),
            v => v == null ? null : Deserialize(v))
    {
    }

    private static EntitlementSnapshot Deserialize(string json)
    {
        return JsonSerializer.Deserialize<EntitlementSnapshot>(json, SerializerOptions)
               ?? throw new JsonException("Failed to deserialize EntitlementSnapshot.");
    }
}

