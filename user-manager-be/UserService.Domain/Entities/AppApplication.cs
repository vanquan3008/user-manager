using UserService.Domain.Exceptions;
using UserService.Domain.ValueObjects;
using UserService.Domain.Events;
using UserService.Domain.Constants;

namespace UserService.Domain.Entities;

public sealed class AppApplication : AuditableAggregateRoot<Guid>
{
    private readonly HashSet<UrlValue> _allowedRedirectUris = new();

    private AppApplication() : base() { }

    private AppApplication(
        Guid id,
        Guid ownerId,
        AppName name,
        AppDescription description,
        ApiKey apiKey,
        ApiSecretHash apiSecretHash,
        string clientType,
        UrlValue? logoUrl,
        Guid? subscriptionPlanId,
        DateTime createdAt) : base()
    {
        Id = id;
        OwnerId = ownerId;
        Name = name;
        Description = description;
        ApiKey = apiKey;
        ApiSecretHash = apiSecretHash;
        ClientType = ApplicationClientTypes.Normalize(clientType);
        LogoUrl = logoUrl;
        SubscriptionPlanId = subscriptionPlanId;
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
        string clientType,
        DateTime createdAt,
        UrlValue? logoUrl = null,
        Guid? subscriptionPlanId = null)
    {
        var app = new AppApplication(
            id,
            ownerId,
            name,
            description,
            apiKey,
            apiSecretHash,
            clientType,
            logoUrl,
            subscriptionPlanId,
            createdAt);

        app.AddDomainEvent(new AppApplicationCreatedDomainEvent(app.Id, ownerId, createdAt));

        return app;
    }

    public AppName Name { get; private set; } = null!;
    public AppDescription Description { get; private set; } = null!;
    
    public Guid OwnerId { get; private set; }
    public User Owner { get; private set; } = null!;

    public Guid? SubscriptionPlanId { get; private set; }
    public SubscriptionPlan? SubscriptionPlan { get; private set; }

    public ApiKey ApiKey { get; private set; } = null!;
    public ApiSecretHash ApiSecretHash { get; private set; } = null!;
    public string ClientType { get; private set; } = ApplicationClientTypes.Public;
    
    public UrlValue? LogoUrl { get; private set; }
    public DateTime? LastApiKeyRotatedAt { get; private set; }
    
    public UrlValue? WebhookUrl { get; private set; }
    public bool IsWebhookEnabled { get; private set; }
    
    public UrlValue? BackchannelLogoutUri { get; private set; }
    public bool IsActive { get; private set; }

    public IReadOnlyCollection<UrlValue> AllowedRedirectUris => _allowedRedirectUris;
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

    public void SetClientType(string clientType)
    {
        ClientType = ApplicationClientTypes.Normalize(clientType);
        Touch();
    }

    public void UpdateSubscriptionPlan(Guid? newPlanId)
    {
        SubscriptionPlanId = newPlanId;
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

    public void RotateClientSecret(ApiSecretHash newSecretHash, DateTime rotatedAt)
    {
        ApiSecretHash = newSecretHash ?? throw new ArgumentNullException(nameof(newSecretHash));
        LastApiKeyRotatedAt = rotatedAt;
        Touch();
    }

    public bool UsesClientSecret()
        => ApplicationClientTypes.UsesClientSecret(ClientType);

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
    public void EnsureActive()
    {
        if (!IsActive)
            throw new ApplicationInactiveException(Id);
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
