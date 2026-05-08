using UserService.Domain.Exceptions;

namespace UserService.Domain.Entities;


public sealed class OAuthAuthorizationCode : AggregateRoot<Guid>
{
    private OAuthAuthorizationCode() : base() { }

    public OAuthAuthorizationCode(
        Guid id,
        string code,
        Guid userId,
        Guid applicationId,
        string redirectUri,
        string scopes,
        string codeChallenge,
        string codeChallengeMethod,
        DateTime expiresAtUtc)
        : base()
    {
        Id = id;
        Code = code ?? throw new ArgumentNullException(nameof(code));
        UserId = userId;
        ApplicationId = applicationId;
        RedirectUri = redirectUri ?? throw new ArgumentNullException(nameof(redirectUri));
        Scopes = scopes ?? string.Empty;
        CodeChallenge = codeChallenge ?? throw new ArgumentNullException(nameof(codeChallenge));
        CodeChallengeMethod = codeChallengeMethod ?? "S256";
        ExpiresAtUtc = expiresAtUtc;
        MarkCreated();
    }

    public string Code { get; private set; } = string.Empty;
    public Guid UserId { get; private set; }
    public Guid ApplicationId { get; private set; }
    public string RedirectUri { get; private set; } = string.Empty;
    public string Scopes { get; private set; } = string.Empty; 
    public string CodeChallenge { get; private set; } = string.Empty;
    public string CodeChallengeMethod { get; private set; } = "S256";
    public DateTime ExpiresAtUtc { get; private set; }
    public DateTime? ConsumedAtUtc { get; private set; }

    public User User { get; private set; } = null!;
    public AppApplication Application { get; private set; } = null!;

    public bool IsExpired(DateTime nowUtc) => nowUtc >= ExpiresAtUtc;
    public bool IsConsumed => ConsumedAtUtc.HasValue;

    public void Consume(DateTime nowUtc)
    {
        if (IsConsumed)
            throw new DomainOperationNotAllowedException("ConsumeAuthorizationCode", "Authorization code already consumed.");

        ConsumedAtUtc = nowUtc;
        Touch();
    }
}

