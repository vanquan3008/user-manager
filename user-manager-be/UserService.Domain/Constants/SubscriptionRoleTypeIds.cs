namespace UserService.Domain.Constants;

public static class SubscriptionRoleTypeIds
{
    public const string OwnerName = "Owner";
    public const string AdminName = "Admin";
    public const string MemberName = "Member";

    public static readonly Guid Owner = CreateDeterministicGuid(OwnerName);
    public static readonly Guid Admin = CreateDeterministicGuid(AdminName);
    public static readonly Guid Member = CreateDeterministicGuid(MemberName);

    public static readonly string[] AllRoles = 
    {
        OwnerName,
        AdminName,
        MemberName
    };

    public static bool IsAdmin(string? roleName) =>
        string.Equals(roleName, OwnerName, StringComparison.OrdinalIgnoreCase) ||
        string.Equals(roleName, AdminName, StringComparison.OrdinalIgnoreCase);

    public static bool IsOwner(string? roleName) =>
        string.Equals(roleName, OwnerName, StringComparison.OrdinalIgnoreCase);
    private static Guid CreateDeterministicGuid(string input)
    {
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var hash = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes($"SubscriptionRole:{input}"));
        var guidBytes = new byte[16];
        Array.Copy(hash, guidBytes, 16);
        return new Guid(guidBytes);
    }
}
