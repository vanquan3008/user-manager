namespace UserService.Domain.Entities;

/// <summary>
/// Entity - Consent của user cho một application (OAuth client).
/// </summary>
public sealed class UserOAuthConsent : AggregateRoot<Guid>
{
    private readonly List<UserOAuthConsentScope> _scopes = new();

    private UserOAuthConsent() : base() { }

    public UserOAuthConsent(Guid id, Guid userId, Guid applicationId)
        : base()
    {
        Id = id;
        UserId = userId;
        ApplicationId = applicationId;
        IsActive = true;
        MarkCreated();
    }

    public Guid UserId { get; private set; }
    public Guid ApplicationId { get; private set; }
    public bool IsActive { get; private set; }

    public User User { get; private set; } = null!;
    public AppApplication Application { get; private set; } = null!;
    public IReadOnlyCollection<UserOAuthConsentScope> Scopes => _scopes.AsReadOnly();

    public void GrantScope(Guid permissionId)
    {
        if (_scopes.Any(s => s.PermissionId == permissionId))
            return;

        _scopes.Add(new UserOAuthConsentScope(Guid.NewGuid(), Id, permissionId));
        Touch();
    }

    public void ReplaceScopes(IEnumerable<Guid> permissionIds)
    {
        _scopes.Clear();
        foreach (var permissionId in permissionIds.Distinct())
        {
            _scopes.Add(new UserOAuthConsentScope(Guid.NewGuid(), Id, permissionId));
        }
        Touch();
    }

    public void Activate()
    {
        IsActive = true;
        Touch();
    }

    public void Deactivate()
    {
        IsActive = false;
        Touch();
    }
}

