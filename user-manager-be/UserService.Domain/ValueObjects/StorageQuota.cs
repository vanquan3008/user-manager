namespace UserService.Domain.ValueObjects;

public sealed record StorageQuota
{
    public long Megabytes { get; }

    private StorageQuota(long megabytes) => Megabytes = megabytes;

    public static StorageQuota Create(long megabytes)
    {
        if (megabytes < 0)
            throw new ArgumentException("Storage quota cannot be negative.", nameof(megabytes));

        return new StorageQuota(megabytes);
    }
    
    public static implicit operator long(StorageQuota quota) => quota.Megabytes;
}
