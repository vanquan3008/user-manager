using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Serilog;
using UserService.Domain.Constants;
using UserService.Domain.Entities;
using UserService.Domain.ValueObjects;
using UserService.Infrastructure.Data;

namespace UserService.Infrastructure.Seeding;

public class PermissionSeeder
{
    private readonly UserDbContext _db;
    private readonly ILogger _log = Log.ForContext<PermissionSeeder>();

    public PermissionSeeder(UserDbContext db)
    {
        _db = db;
    }

    public async Task SeedPermissionsAsync(CancellationToken ct = default)
    {
        var codes = GetPermissionCodes();
        _log.Information("Seeding {Count} permissions from PermissionCodes", codes.Count);

        foreach (var code in codes)
        {
            if (await _db.Permissions.AnyAsync(p => p.Code == code, ct))
                continue;

            var (description, category) = FromCode(code);
            var permission = Permission.Create(PermissionCode.Create(code), description, category, isSystemPermission: true);
            await _db.Permissions.AddAsync(permission, ct);
        }

        await _db.SaveChangesAsync(ct);
        _log.Information("Permission seeding completed");
    }

    private static List<string> GetPermissionCodes()
    {
        var type = typeof(PermissionCodes);

        var codes = new List<string>();
        foreach (var f in type.GetFields(BindingFlags.Public | BindingFlags.Static))
        {
            if (f.FieldType != typeof(string)) continue;
            var val = f.GetValue(null) as string;
            if (string.IsNullOrEmpty(val)) continue;
            codes.Add(val);
        }
        return codes.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
    }

    private static (string Description, string Category) FromCode(string code)
    {
        var parts = code.Split('.', StringSplitOptions.RemoveEmptyEntries);

        var resource = parts[0];
        var action = parts[1];

        var verb = char.ToUpperInvariant(action[0]) +
                   action[1..].ToLowerInvariant();

        var cat = char.ToUpperInvariant(resource[0]) +
                  resource[1..].ToLowerInvariant();

        return ($"{verb} {cat}", cat);
    } 
}
