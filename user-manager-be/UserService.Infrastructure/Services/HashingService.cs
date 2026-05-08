using UserService.Domain.Interfaces;

namespace UserService.Infrastructure.Services;

public sealed class HashingService : IHashingService
{
    public string Hash(string plainText)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(plainText);

        return BCrypt.Net.BCrypt.HashPassword(
            plainText,
            workFactor: 12);
    }
    public bool Verify(string plainText, string hash)
    {
        if (string.IsNullOrWhiteSpace(plainText))
            return false;

        if (string.IsNullOrWhiteSpace(hash))
            return false;
        try
        {
            return BCrypt.Net.BCrypt.Verify(plainText, hash);
        }
        catch
        {
            return false;
        }
    }
}
