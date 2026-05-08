using UserService.Domain.ValueObjects;

namespace UserService.Domain.Entities;

public sealed class OAuthRefreshToken : AggregateRoot<Guid>
{
    private OAuthRefreshToken() : base() { }

    private OAuthRefreshToken(
        Guid id,
        Guid userId,
        Guid applicationId,
        TokenValue tokenHash,
        string scopes,
        DateTime expiresAtUtc)
        : base()
    {
        Id = id;
        UserId = userId;
        ApplicationId = applicationId;
        TokenHash = tokenHash ?? throw new ArgumentNullException(nameof(tokenHash));
        Scopes = scopes ?? string.Empty;
        ExpiresAtUtc = expiresAtUtc;
        MarkCreated();
    }

    public static OAuthRefreshToken Create(
        Guid id,
        Guid userId,
        Guid applicationId,
        TokenValue tokenHash,
        string scopes,
        DateTime expiresAtUtc)
    {
        return new OAuthRefreshToken(id, userId, applicationId, tokenHash, scopes, expiresAtUtc);
    }

    public Guid UserId { get; private set; }
    public Guid ApplicationId { get; private set; }
    public TokenValue TokenHash { get; private set; } = null!;
    public string Scopes { get; private set; } = string.Empty;
    
    public DateTime ExpiresAtUtc { get; private set; }
    public DateTime? RevokedAtUtc { get; private set; }

    public User User { get; private set; } = null!;
    public AppApplication Application { get; private set; } = null!;

    public bool IsExpired(DateTime utcNow) => utcNow >= ExpiresAtUtc;
    public bool IsRevoked => RevokedAtUtc.HasValue;

    public void Revoke(DateTime revokedAt)
    {
        if (IsRevoked)
            return;

        RevokedAtUtc = revokedAt;
        Touch();
    }
}
