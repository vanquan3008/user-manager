using UserService.Domain.Exceptions;

namespace UserService.Domain.Entities;

/// <summary>
/// Single-use OAuth 2.0 authorization code (RFC 6749 §4.1) bound to a PKCE challenge (RFC 7636).
/// Only the SHA-256 hash of the code is persisted.
/// </summary>
public sealed class OAuthAuthorizationCode : AggregateRoot<Guid>
{
    private OAuthAuthorizationCode() : base() { }

    public OAuthAuthorizationCode(
        Guid id,
        string codeHash,
        Guid userId,
        Guid applicationId,
        string redirectUri,
        string scopes,
        string codeChallenge,
        string codeChallengeMethod,
        DateTime expiresAtUtc,
        string? nonce = null,
        Guid? sessionId = null)
        : base()
    {
        Id = id;
        CodeHash = codeHash ?? throw new ArgumentNullException(nameof(codeHash));
        UserId = userId;
        ApplicationId = applicationId;
        RedirectUri = redirectUri ?? throw new ArgumentNullException(nameof(redirectUri));
        Scopes = scopes ?? string.Empty;
        CodeChallenge = codeChallenge ?? throw new ArgumentNullException(nameof(codeChallenge));
        CodeChallengeMethod = codeChallengeMethod ?? "S256";
        ExpiresAtUtc = expiresAtUtc;
        Nonce = string.IsNullOrWhiteSpace(nonce) ? null : nonce.Trim();
        SessionId = sessionId;
        Version = Guid.NewGuid();
        MarkCreated();
    }

    public string CodeHash { get; private set; } = string.Empty;
    public Guid UserId { get; private set; }
    public Guid ApplicationId { get; private set; }
    public string RedirectUri { get; private set; } = string.Empty;
    public string Scopes { get; private set; } = string.Empty; 
    public string CodeChallenge { get; private set; } = string.Empty;
    public string CodeChallengeMethod { get; private set; } = "S256";
    public string? Nonce { get; private set; }
    /// <summary>The hosted SSO session that authorized this code (used for single logout).</summary>
    public Guid? SessionId { get; private set; }
    public DateTime ExpiresAtUtc { get; private set; }
    public DateTime? ConsumedAtUtc { get; private set; }
    public Guid Version { get; private set; }

    public User User { get; private set; } = null!;
    public AppApplication Application { get; private set; } = null!;

    public bool IsExpired(DateTime nowUtc) => nowUtc >= ExpiresAtUtc;
    public bool IsConsumed => ConsumedAtUtc.HasValue;

    public void Consume(DateTime nowUtc)
    {
        if (IsConsumed)
            throw new DomainOperationNotAllowedException("ConsumeAuthorizationCode", "Authorization code already consumed.");

        ConsumedAtUtc = nowUtc;
        Version = Guid.NewGuid();
        Touch();
    }
}
