using UserService.Domain.Exceptions;
using UserService.Domain.ValueObjects;
using UserService.Domain.Events;

namespace UserService.Domain.Entities;

public sealed class AppApplication : AuditableAggregateRoot<Guid>
{
    private readonly HashSet<UserApplication> _users = new();
    private readonly HashSet<UrlValue> _allowedRedirectUris = new();

    private AppApplication() : base() { }

    private AppApplication(
        Guid id,
        Guid ownerId,
        AppName name,
        AppDescription description,
        ApiKey apiKey,
        ApiSecretHash apiSecretHash,
        UrlValue? logoUrl,
        Guid? subscriptionPlanId,
        Guid? defaultLicensePackageId,
        DateTime createdAt) : base()
    {
        Id = id;
        OwnerId = ownerId;
        Name = name;
        Description = description;
        ApiKey = apiKey;
        ApiSecretHash = apiSecretHash;
        LogoUrl = logoUrl;
        SubscriptionPlanId = subscriptionPlanId;
        DefaultLicensePackageId = defaultLicensePackageId;
        IsActive = true;
        
        MarkCreated();
    }

    public static AppApplication Create(
        Guid id,
        Guid ownerId,
        AppName name,
        AppDescription description,
        ApiKey apiKey,
        ApiSecretHash apiSecretHash,
        DateTime createdAt,
        UrlValue? logoUrl = null,
        Guid? subscriptionPlanId = null,
        Guid? defaultLicensePackageId = null)
    {
        var app = new AppApplication(
            id,
            ownerId,
            name,
            description,
            apiKey,
            apiSecretHash,
            logoUrl,
            subscriptionPlanId,
            defaultLicensePackageId,
            createdAt);

        app.AddUser(ownerId, "Admin", createdAt);
        app.AddDomainEvent(new AppApplicationCreatedDomainEvent(app.Id, ownerId, createdAt));

        return app;
    }

    public AppName Name { get; private set; } = null!;
    public AppDescription Description { get; private set; } = null!;
    
    public Guid OwnerId { get; private set; }
    public User Owner { get; private set; } = null!;

    public Guid? DefaultLicensePackageId { get; private set; }
    public LicensePackage? DefaultLicensePackage { get; private set; }

    public Guid? SubscriptionPlanId { get; private set; }
    public SubscriptionPlan? SubscriptionPlan { get; private set; }

    public ApiKey ApiKey { get; private set; } = null!;
    public ApiSecretHash ApiSecretHash { get; private set; } = null!;
    
    public UrlValue? LogoUrl { get; private set; }
    public DateTime? LastApiKeyRotatedAt { get; private set; }
    
    public UrlValue? WebhookUrl { get; private set; }
    public bool IsWebhookEnabled { get; private set; }
    
    public UrlValue? BackchannelLogoutUri { get; private set; }
    public bool IsActive { get; private set; }

    public IReadOnlyCollection<UrlValue> AllowedRedirectUris => _allowedRedirectUris;
    public IReadOnlyCollection<UserApplication> Users => _users;

    public void UpdateInfo(AppName name, AppDescription description)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Description = description ?? throw new ArgumentNullException(nameof(description));
        Touch();
    }

    public void SetLogoUrl(UrlValue? logoUrl)
    {
        LogoUrl = logoUrl;
        Touch();
    }

    public void SetDefaultLicensePackage(Guid? licensePackageId)
    {
        DefaultLicensePackageId = licensePackageId;
        Touch();
    }

    public void SetApiKey(ApiKey apiKey)
    {
        ApiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
        Touch();
    }

    public void SetApiSecretHash(ApiSecretHash secretHash)
    {
        ApiSecretHash = secretHash ?? throw new ArgumentNullException(nameof(secretHash));
        Touch();
    }

    public void UpdateSubscriptionPlan(Guid? newPlanId)
    {
        SubscriptionPlanId = newPlanId;
        Touch();
    }

    public void AddUser(Guid userId, string role, DateTime occurredOn)
    {
        if (_users.Any(u => u.UserId == userId))
            throw new DomainOperationNotAllowedException("AddUser", "User already exists in this application");

        _users.Add(UserApplication.Create(Guid.NewGuid(), userId, this.Id, role));
        
        AddDomainEvent(new UserAddedToApplicationDomainEvent(this.Id, userId, role, occurredOn));
        Touch();
    }

    public void RemoveUser(Guid userId)
    {
        var user = _users.FirstOrDefault(u => u.UserId == userId);
        if (user == null)
            throw new DomainOperationNotAllowedException("RemoveUser", "User not found in this application");

        if (user.Role == "Admin" && _users.Count(u => u.Role == "Admin") <= 1)
            throw new DomainOperationNotAllowedException("RemoveUser", "Cannot remove the last admin user");

        _users.Remove(user);
        Touch();
    }

    public void RotateApiKeys(ApiKey newApiKey, ApiSecretHash newSecretHash, DateTime rotatedAt, out string generatedPlainApiSecret)
    {
        ApiKey = newApiKey;
        generatedPlainApiSecret = ApiSecretGenerator.Generate();
        ApiSecretHash = newSecretHash;
        
        LastApiKeyRotatedAt = rotatedAt;
        
        AddDomainEvent(new ApiKeyRotatedDomainEvent(this.Id, rotatedAt));
        Touch();
    }

    public void SetWebhook(UrlValue webhookUrl, bool enabled)
    {
        WebhookUrl = webhookUrl ?? throw new ArgumentNullException(nameof(webhookUrl));
        IsWebhookEnabled = enabled;
        Touch();
    }

    public void AddRedirectUri(UrlValue redirectUri)
    {
        if (_allowedRedirectUris.Contains(redirectUri))
            throw new DomainOperationNotAllowedException("AddRedirectUri", "Redirect URI already exists.");
            
        _allowedRedirectUris.Add(redirectUri);
        Touch();
    }

    public void RemoveRedirectUri(UrlValue redirectUri)
    {
        _allowedRedirectUris.Remove(redirectUri);
        Touch();
    }

    public void SetBackchannelLogoutUri(UrlValue? backchannelLogoutUri)
    {
        BackchannelLogoutUri = backchannelLogoutUri;
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

    public void Delete()
    {
        MarkDeleted();
    }
}
