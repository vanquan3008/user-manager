namespace UserService.Domain.ValueObjects;

public sealed record ApiSecretHash
{
    public string Value { get; }

    private ApiSecretHash(string value) => Value = value;

    public static ApiSecretHash Create(string hash)
    {
        if (string.IsNullOrWhiteSpace(hash))
            throw new ArgumentException("API Secret Hash cannot be empty.", nameof(hash));

        return new ApiSecretHash(hash);
    }
    
    public static implicit operator string(ApiSecretHash hash) => hash.Value;
}

public static class ApiSecretGenerator
{
    public static string Generate()
    {
        var bytes1 = Guid.NewGuid().ToByteArray();
        var bytes2 = Guid.NewGuid().ToByteArray();
        var combined = new byte[bytes1.Length + bytes2.Length];
        Buffer.BlockCopy(bytes1, 0, combined, 0, bytes1.Length);
        Buffer.BlockCopy(bytes2, 0, combined, bytes1.Length, bytes2.Length);
        return Convert.ToBase64String(combined);
    }
}
