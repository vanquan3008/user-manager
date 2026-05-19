using UserService.Domain.ValueObjects;

namespace UserService.Domain.Entities;

public sealed class MagicLinkToken : Entity<Guid>
{
    private MagicLinkToken() { }

    private MagicLinkToken(
        Guid userId, 
        TokenValue token, 
        string purpose, 
        DateTime createdAt,
        int expiryMinutes = 15,
        string? email = null,
        string? ipAddress = null,
        string? userAgent = null)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        Token = token ?? throw new ArgumentNullException(nameof(token));
        Purpose = purpose ?? throw new ArgumentNullException(nameof(purpose));
        Email = email;
        ExpiresAt = createdAt.AddMinutes(expiryMinutes);
        IsUsed = false;
        IpAddress = ipAddress;
        UserAgent = userAgent;
        CreatedAt = createdAt;
    }
    
    public static MagicLinkToken Create(
        Guid userId, 
        TokenValue token, 
        string purpose, 
        DateTime createdAt,
        int expiryMinutes = 15,
        string? email = null,
        string? ipAddress = null,
        string? userAgent = null)
    {
        return new MagicLinkToken(userId, token, purpose, createdAt, expiryMinutes, email, ipAddress, userAgent);
    }

    public Guid UserId { get; private set; }

    public User User { get; private set; } = null!;
    
    public TokenValue Token { get; private set; } = null!;
    public string Purpose { get; private set; } = string.Empty;
    public string? Email { get; private set; }
    
    public DateTime ExpiresAt { get; private set; }
    public bool IsUsed { get; private set; }
    public DateTime? UsedAt { get; private set; }
    
    public string? IpAddress { get; private set; }
    public string? UserAgent { get; private set; }
    public bool IsValid(DateTime utcNow)
    {
        return !IsUsed && utcNow <= ExpiresAt;
    }

    public void MarkAsUsed(DateTime usedAt)
    {
        IsUsed = true;
        UsedAt = usedAt;
    }
}
