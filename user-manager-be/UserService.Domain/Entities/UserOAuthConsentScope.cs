namespace UserService.Domain.Entities;

public sealed class UserOAuthConsentScope : Entity<Guid>
{
    private UserOAuthConsentScope() : base() { }

    public UserOAuthConsentScope(Guid id, Guid consentId, Guid permissionId)
        : base()
    {
        Id = id;
        ConsentId = consentId;
        PermissionId = permissionId;
        MarkCreated();
    }

    public Guid ConsentId { get; private set; }
    public Guid PermissionId { get; private set; }
    public UserOAuthConsent Consent { get; private set; } = null!;
    public Permission Permission { get; private set; } = null!;
}

