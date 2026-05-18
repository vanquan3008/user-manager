using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using UserService.Domain.Entities;
using UserService.Domain.Events;
using UserService.Domain.Interfaces;
using UserService.Infrastructure.Identity;

namespace UserService.Infrastructure.Data;

public sealed class UserDbContext : IdentityDbContext<
    AppUser, AppRole, Guid, 
    IdentityUserClaim<Guid>,
    AppUserRole,
    IdentityUserLogin<Guid>,
    IdentityRoleClaim<Guid>,
    IdentityUserToken<Guid>>
{
    private readonly IClock _clock;
    public UserDbContext(
        DbContextOptions<UserDbContext> options, 
        IClock clock)
        : base(options)
    {
        ArgumentNullException.ThrowIfNull(clock);
        _clock = clock;
    }

    public DbSet<User> DomainUsers { get; set; } = null!;
    public DbSet<MagicLinkToken> MagicLinkTokens { get; set; } = null!;
    public DbSet<Subscription> Subscriptions { get; set; } = null!;
    public DbSet<SubscriptionPlan> SubscriptionPlans { get; set; } = null!;
    public DbSet<SubscriptionPlanPermission> SubscriptionPlanPermissions { get; set; } = null!;
    public DbSet<ApplicationSubscription> ApplicationSubscriptions { get; set; } = null!;
    public DbSet<AppApplication> AppApplications { get; set; } = null!;
    public DbSet<ApplicationMembership> ApplicationMemberships { get; set; } = null!;
    public DbSet<Feature> Features { get; set; } = null!;
    public DbSet<PlanFeature> PlanFeatures { get; set; } = null!;
    public DbSet<Permission> Permissions { get; set; } = null!;
    public DbSet<RolePermission> RolePermissions { get; set; } = null!;
    public DbSet<ApplicationRole> ApplicationRoles { get; set; } = null!;
    public DbSet<ApplicationOAuthScope> ApplicationOAuthScopes { get; set; } = null!;
    public DbSet<UserOAuthConsent> UserOAuthConsents { get; set; } = null!;
    public DbSet<UserOAuthConsentScope> UserOAuthConsentScopes { get; set; } = null!;
    public DbSet<OAuthAuthorizationCode> OAuthAuthorizationCodes { get; set; } = null!;
    public DbSet<OAuthRefreshToken> OAuthRefreshTokens { get; set; } = null!;
    public DbSet<OutboxMessage> OutboxMessages { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(UserDbContext).Assembly);
        builder.ApplySoftDeleteQueryFilters();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var utcNow = _clock.UtcNow;

        ApplyAuditing(utcNow);
        ApplySoftDelete(utcNow);

        var domainEvents = ExtractDomainEvents();
        EnqueueOutboxMessages(domainEvents);

        return await base.SaveChangesAsync(cancellationToken);
    }

    private void ApplyAuditing(DateTime utcNow)
    {
        var auditableEntries = ChangeTracker.Entries<IAuditable>()
            .Where(e => e.State is EntityState.Added or EntityState.Modified)
            .ToList();

        foreach (var entry in auditableEntries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.SetCreatedAt(utcNow);
            }
            else
            {
                entry.Entity.SetUpdatedAt(utcNow);
            }
        }
    }

    private void ApplySoftDelete(DateTime utcNow)
    {
        var softDeleteEntries = ChangeTracker.Entries<ISoftDelete>()
            .Where(e => e.State is EntityState.Deleted or EntityState.Modified)
            .ToList();

        foreach (var entry in softDeleteEntries)
        {
            if (entry.State == EntityState.Deleted)
            {
                entry.State = EntityState.Modified;
                entry.Entity.SoftDelete(utcNow);
                continue;
            }
            if (entry.Entity.IsDeleted && entry.Entity.DeletedAt is null)
            {
                entry.Entity.SoftDelete(utcNow);
            }
        }
    }

    private List<IDomainEvent> ExtractDomainEvents()
    {
        var aggregates = ChangeTracker.Entries<IAggregateRoot>()
            .Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .Select(e => e.Entity)
            .Where(a => a.DomainEvents.Count > 0)
            .ToList();

        var domainEvents = aggregates
            .SelectMany(a => a.DomainEvents)
            .ToList();

        foreach (var aggregate in aggregates)
        {
            aggregate.ClearDomainEvents();
        }

        return domainEvents;
    }

    private void EnqueueOutboxMessages(IEnumerable<IDomainEvent> domainEvents)
    {
        foreach (var domainEvent in domainEvents)
        {
            OutboxMessages.Add(OutboxMessage.Create(domainEvent, OutboxJsonSerializer.Serialize(domainEvent)));
        }
    }
}
