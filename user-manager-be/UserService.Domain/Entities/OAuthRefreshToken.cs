using UserService.Domain.ValueObjects;

namespace UserService.Domain.Entities;

/// <summary>
/// Rotating OAuth refresh token issued to a client application. Only the SHA-256 hash is stored.
/// A token revoked because it was rotated must never be presented again; doing so indicates
/// theft and revokes the whole user/client token family (OAuth 2.0 Security BCP §4.14).
/// </summary>
public sealed class OAuthRefreshToken : AggregateRoot<Guid>
{
    public const string RevokedByRotation = "rotated";

    private OAuthRefreshToken() : base() { }

    private OAuthRefreshToken(
        Guid id,
        Guid userId,
        Guid applicationId,
        TokenValue tokenHash,
        string scopes,
        DateTime expiresAtUtc,
        Guid? sessionId)
        : base()
    {
        Id = id;
        UserId = userId;
        ApplicationId = applicationId;
        TokenHash = tokenHash ?? throw new ArgumentNullException(nameof(tokenHash));
        Scopes = scopes ?? string.Empty;
        ExpiresAtUtc = expiresAtUtc;
        SessionId = sessionId;
        Version = Guid.NewGuid();
        MarkCreated();
    }

    public static OAuthRefreshToken Create(
        Guid id,
        Guid userId,
        Guid applicationId,
        TokenValue tokenHash,
        string scopes,
        DateTime expiresAtUtc,
        Guid? sessionId = null)
    {
        return new OAuthRefreshToken(id, userId, applicationId, tokenHash, scopes, expiresAtUtc, sessionId);
    }

    public Guid UserId { get; private set; }
    public Guid ApplicationId { get; private set; }
    public TokenValue TokenHash { get; private set; } = null!;
    public string Scopes { get; private set; } = string.Empty;
    /// <summary>The hosted SSO session the grant belongs to (single logout revokes it).</summary>
    public Guid? SessionId { get; private set; }
    
    public DateTime ExpiresAtUtc { get; private set; }
    public DateTime? RevokedAtUtc { get; private set; }
    public string? RevokedReason { get; private set; }
    public Guid Version { get; private set; }

    public User User { get; private set; } = null!;
    public AppApplication Application { get; private set; } = null!;

    public bool IsExpired(DateTime utcNow) => utcNow >= ExpiresAtUtc;
    public bool IsRevoked => RevokedAtUtc.HasValue;
    public bool WasRotated => string.Equals(RevokedReason, RevokedByRotation, StringComparison.Ordinal);

    public void Revoke(DateTime revokedAt, string reason = "revoked")
    {
        if (IsRevoked)
            return;

        RevokedAtUtc = revokedAt;
        RevokedReason = reason.Length > 50 ? reason[..50] : reason;
        Version = Guid.NewGuid();
        Touch();
    }
}
