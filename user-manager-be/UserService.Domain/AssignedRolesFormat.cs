using UserService.Domain.Constants;

namespace UserService.Domain;

public static class AssignedRolesFormat
{
    public static string Normalize(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return RoleNames.Guest;
        var parts = input
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(s => s.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        return parts.Count == 0 ? RoleNames.Guest : string.Join(",", parts);
    }

    public static IReadOnlyList<string> Split(string? input)
    {
        var n = Normalize(input);
        return n.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
    }
}
