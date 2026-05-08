using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using UserService.Application.Common;
using UserService.Domain.Entities;
using UserService.Domain.Events;
using UserService.Infrastructure.Identity;

using UserService.Domain.Interfaces;

namespace UserService.Infrastructure.Data;

public sealed class UserDbContext : IdentityDbContext<
    AppUser, AppRole, Guid, 
    IdentityUserClaim<Guid>,
    AppUserRole,
    IdentityUserLogin<Guid>,
    IdentityRoleClaim<Guid>,
    IdentityUserToken<Guid>>
{
    private readonly IDomainEventDispatcher _domainEventDispatcher;
    private readonly IClock _clock;
    public UserDbContext(
        DbContextOptions<UserDbContext> options, 
        IDomainEventDispatcher domainEventDispatcher,
        IClock clock)
        : base(options)
    {
        ArgumentNullException.ThrowIfNull(domainEventDispatcher);
        ArgumentNullException.ThrowIfNull(clock);
        _domainEventDispatcher = domainEventDispatcher;
        _clock = clock;
    }

    public DbSet<User> DomainUsers { get; set; } = null!;
    public DbSet<MagicLinkToken> MagicLinkTokens { get; set; } = null!;
    public DbSet<Subscription> Subscriptions { get; set; } = null!;
    public DbSet<SubscriptionMember> SubscriptionMembers { get; set; } = null!;
    public DbSet<SubscriptionRoleType> SubscriptionRoleTypes { get; set; } = null!;
    public DbSet<SubscriptionPlan> SubscriptionPlans { get; set; } = null!;
    public DbSet<LicensePackage> LicensePackages { get; set; } = null!;
    public DbSet<AppApplication> AppApplications { get; set; } = null!;
    public DbSet<UserApplication> UserApplications { get; set; } = null!;
    public DbSet<FamilyPlan> FamilyPlans { get; set; } = null!;
    public DbSet<FamilySubscription> FamilySubscriptions { get; set; } = null!;
    public DbSet<Permission> Permissions { get; set; } = null!;
    public DbSet<RolePermission> RolePermissions { get; set; } = null!;
    public DbSet<ApplicationRole> ApplicationRoles { get; set; } = null!;
    public DbSet<ApplicationOAuthScope> ApplicationOAuthScopes { get; set; } = null!;
    public DbSet<UserOAuthConsent> UserOAuthConsents { get; set; } = null!;
    public DbSet<UserOAuthConsentScope> UserOAuthConsentScopes { get; set; } = null!;
    public DbSet<OAuthAuthorizationCode> OAuthAuthorizationCodes { get; set; } = null!;
    public DbSet<OAuthRefreshToken> OAuthRefreshTokens { get; set; } = null!;

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

        var aggregatesWithEvents = GetAggregatesWithEvents();

        var result = await base.SaveChangesAsync(cancellationToken);

        await DispatchDomainEventsAsync(aggregatesWithEvents, cancellationToken);

        return result;
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

    private List<IAggregateRoot> GetAggregatesWithEvents()
    {
        return ChangeTracker.Entries<IAggregateRoot>()
            .Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .Select(e => e.Entity)
            .Where(a => a.DomainEvents.Count > 0)
            .ToList();
    }

    private async Task DispatchDomainEventsAsync(
        List<IAggregateRoot> aggregatesWithEvents,
        CancellationToken cancellationToken)
    {
        if (aggregatesWithEvents.Count == 0)
        {
            return;
        }
        var domainEvents = aggregatesWithEvents
            .SelectMany(a => a.DomainEvents)
            .ToList();

        if (domainEvents.Count == 0)
        {
            return;
        }

        await _domainEventDispatcher.DispatchAsync(domainEvents, cancellationToken);
        foreach (var aggregate in aggregatesWithEvents)
        {
            aggregate.ClearDomainEvents();
        }
    }
}
