using Microsoft.EntityFrameworkCore;
using Serilog;
using UserService.Application.Configuration;
using UserService.Domain.Entities;
using UserService.Infrastructure.Data;
using Microsoft.Extensions.Configuration;
using UserService.Domain.Constants;
using UserService.Domain.ValueObjects;
using UserService.Domain.Interfaces;
using UserService.Infrastructure.Identity;

namespace UserService.Infrastructure.Seeding;

public class ApplicationManagerSeeder
{
    private readonly UserDbContext _db;
    private readonly AppSettings _appSettings;
    private readonly IConfiguration _configuration;
    private readonly IHashingService _hashingService;
    private readonly IClock _clock;

    public ApplicationManagerSeeder(
        UserDbContext db, 
        AppSettings appSettings, 
        IConfiguration configuration,
        IHashingService hashingService,
        IClock clock)
    {
        _db = db;
        _appSettings = appSettings;
        _configuration = configuration;
        _hashingService = hashingService;
        _clock = clock;
    }

    private string AppName => _configuration["SEED_APP_NAME"]!;
    private string AppDescription => _configuration["SEED_APP_DESCRIPTION"]!;
    private string AppApiKey => _configuration["SEED_APP_API_KEY"]!;
    private const string AdminRole = "Admin";
    private static readonly string[] DefaultOAuthScopes = new[] { "profile", "email" ,"openId" };
    private static readonly string[] DefaultApplicationRoles =
    {
        RoleNames.Admin,
        RoleNames.Guest,
        RoleNames.SuperAdmin
    };

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            Log.Information("Starting application manager seed for '{AppName}'", AppName);

            var existingApp = await _db.AppApplications
                .FirstOrDefaultAsync(a => a.Name == AppName, cancellationToken);

            if (existingApp != null)
            {
                await ConfigureExistingApplicationAsync(existingApp, cancellationToken);
                return;
            }

            await CreateApplicationAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error seeding application manager: {Message}", ex.Message);
            throw;
        }
    }

    private async Task ConfigureExistingApplicationAsync(
        AppApplication app,
        CancellationToken cancellationToken)
    {
        Log.Information(
            "Application '{AppName}' already exists with ID: {AppId}, ApiKey: {ApiKey}",
            AppName,
            app.Id,
            app.ApiKey);

        ApplyDefaults(app);
        await _db.SaveChangesAsync(cancellationToken);
        await EnsureDefaultOAuthScopesAsync(app.Id, onlyWhenMissing: true, cancellationToken);
        await EnsureDefaultApplicationRolesAsync(app.Id, cancellationToken);
        await EnsureAdminUserApplicationAsync(app.Id, cancellationToken);
        Log.Information("Application '{AppName}' configuration completed. ApiKey: {ApiKey}", AppName, AppApiKey);
    }

    private async Task CreateApplicationAsync(CancellationToken cancellationToken)
    {
        var totalUserCount = await _db.DomainUsers.CountAsync(cancellationToken);
        var ssoUserCount = await _db.DomainUsers.CountAsync(u => u.IsSSOAccount, cancellationToken);
        Log.Information(
            "Found {TotalCount} total users, {SSOCount} SSO users in database",
            totalUserCount,
            ssoUserCount);

        var adminUser = await GetPreferredUserAsync(cancellationToken);
        if (adminUser == null)
        {
            Log.Warning("No user found in database, skipping application manager seed. Please ensure admin user is created first.");
            return;
        }

        await EnsureSsoEnabledAsync(adminUser, cancellationToken);
        Log.Information("Using user {UserId} ({Email}) as application owner", adminUser.Id, adminUser.Email.Value);
        var plainSecret = GenerateRandomSecret();
        var apiKey = ApiKey.Create(AppApiKey);
        var apiSecretHash = ApiSecretHash.Create(_hashingService.Hash(plainSecret));

        var app = AppApplication.Create(
            id: Guid.NewGuid(),
            ownerId: adminUser.Id,
            name: UserService.Domain.ValueObjects.AppName.Create(AppName),
            description: UserService.Domain.ValueObjects.AppDescription.Create(AppDescription),
            apiKey,
            apiSecretHash,
            _clock.UtcNow);
        
        foreach (var uri in GetDefaultAllowedRedirectUris())
        {
            app.AddRedirectUri(uri);
        }
        await _db.AppApplications.AddAsync(app, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);

        Log.Information(
            "Application '{AppName}' seeded successfully with ID: {AppId}, Owner: {OwnerId}, ApiKey: {ApiKey}",
            AppName,
            app.Id,
            adminUser.Id,
            AppApiKey);

        await EnsureDefaultOAuthScopesAsync(app.Id, onlyWhenMissing: false, cancellationToken);
        await EnsureDefaultApplicationRolesAsync(app.Id, cancellationToken);
        await EnsureAdminUserApplicationAsync(app.Id, cancellationToken);
    }

    private void ApplyDefaults(AppApplication app)
    {
        if (app.ApiKey.Value != AppApiKey)
        {
            app.SetApiKey(ApiKey.Create(AppApiKey));
            Log.Information("Updated ApiKey to '{ApiKey}' for application '{AppName}'", AppApiKey, AppName);
        }

        if (!app.AllowedRedirectUris.Any())
        {
            foreach (var uri in GetDefaultAllowedRedirectUris())
            {
                app.AddRedirectUri(uri);
            }
            Log.Information("Updated redirect URIs for application '{AppName}'", AppName);
        }

        if (!app.IsActive)
        {
            app.Activate();
            Log.Information("Activated application '{AppName}'", AppName);
        }
    }

    private async Task EnsureDefaultOAuthScopesAsync(
        Guid applicationId,
        bool onlyWhenMissing,
        CancellationToken cancellationToken)
    {
        var oauthScopeSeeder = new OAuthScopeSeeder(_db);

        if (onlyWhenMissing)
        {
            var existingScopes = await _db.ApplicationOAuthScopes
                .Where(s => s.ApplicationId == applicationId && s.IsActive)
                .ToListAsync(cancellationToken);

            Log.Information("Found {Count} existing scopes for application '{AppName}'", existingScopes.Count, AppName);

            if (existingScopes.Count > 0)
            {
                Log.Information("Application '{AppName}' already has {Count} scopes configured", AppName, existingScopes.Count);
                return;
            }

            Log.Information("No scopes found, adding default scopes (profile, email)...");
        }

        foreach (var scope in DefaultOAuthScopes)
        {
            await AddOAuthScopeAsync(oauthScopeSeeder, applicationId, scope, cancellationToken);
        }

        await _db.SaveChangesAsync(cancellationToken);

        if (onlyWhenMissing)
        {
            var scopeCount = await _db.ApplicationOAuthScopes
                .Where(s => s.ApplicationId == applicationId && s.IsActive)
                .CountAsync(cancellationToken);

            Log.Information(
                "OAuth scopes saved successfully for application '{AppName}'. Total scopes now: {Count}",
                AppName,
                scopeCount);
        }
    }

    private async Task EnsureDefaultApplicationRolesAsync(Guid applicationId, CancellationToken cancellationToken)
    {
        var roles = await _db.Set<AppRole>()
            .Where(role => role.Name != null && DefaultApplicationRoles.Contains(role.Name))
            .ToListAsync(cancellationToken);

        foreach (var roleName in DefaultApplicationRoles)
        {
            var role = roles.FirstOrDefault(r => string.Equals(r.Name, roleName, StringComparison.OrdinalIgnoreCase));
            if (role is null)
            {
                Log.Warning("Default application role '{RoleName}' skipped because it does not exist in AspNetRoles.", roleName);
                continue;
            }

            var exists = await _db.ApplicationRoles.AnyAsync(
                mapping => mapping.ApplicationId == applicationId && mapping.RoleId == role.Id,
                cancellationToken);

            if (exists)
                continue;

            await _db.ApplicationRoles.AddAsync(new ApplicationRole(applicationId, role.Id), cancellationToken);
            Log.Information("Added default application role '{RoleName}' to application '{AppName}'", roleName, AppName);
        }

        await _db.SaveChangesAsync(cancellationToken);
    }

    private async Task AddOAuthScopeAsync(
        OAuthScopeSeeder oauthScopeSeeder,
        Guid applicationId,
        string scope,
        CancellationToken cancellationToken)
    {
        var permission = await oauthScopeSeeder.GetPermissionByScopeAsync(scope, cancellationToken);

        Log.Information(
            "{Scope} permission: {PermissionId}",
            scope,
            permission?.Id.ToString() ?? "NULL");

        if (permission == null)
        {
            Log.Error(
                "CRITICAL: {Scope} permission not found! OAuth scope '{Scope}' must exist in Permissions table with Scope='{Scope}'",
                scope);
            return;
        }

        var appScope = new ApplicationOAuthScope(Guid.NewGuid(), applicationId, permission.Id);
        await _db.ApplicationOAuthScopes.AddAsync(appScope, cancellationToken);
        Log.Information(
            "Added '{Scope}' scope (PermissionId: {PermissionId}) to application '{AppName}'",
            scope,
            permission.Id,
            AppName);
    }

    private async Task EnsureAdminUserApplicationAsync(Guid applicationId, CancellationToken cancellationToken)
    {
        var adminUser = await GetPreferredUserAsync(cancellationToken);
        if (adminUser == null)
            return;

        var existingUserApp = await _db.UserApplications
            .AsNoTracking()
            .FirstOrDefaultAsync(
                ua => ua.UserId == adminUser.Id && ua.ApplicationId == applicationId,
                cancellationToken);

        if (existingUserApp != null)
        {
            Log.Information("Admin user {UserId} already in application '{AppName}'", adminUser.Id, AppName);
            return;
        }

        await AddAdminUserApplicationAsync(applicationId, adminUser.Id, cancellationToken);
        Log.Information("Added admin user {UserId} to application '{AppName}'", adminUser.Id, AppName);
    }

    private async Task AddAdminUserApplicationAsync(
        Guid applicationId,
        Guid userId,
        CancellationToken cancellationToken)
    {
        var userApp = UserApplication.Create(
            id: Guid.NewGuid(),
            userId: userId,
            applicationId: applicationId,
            role: AdminRole);

        await _db.UserApplications.AddAsync(userApp, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
    }

    private async Task<User?> GetPreferredUserAsync(CancellationToken cancellationToken)
    {
        return await _db.DomainUsers
            .AsNoTracking()
            .OrderByDescending(u => u.IsSSOAccount)
            .ThenBy(u => u.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    private async Task EnsureSsoEnabledAsync(User adminUser, CancellationToken cancellationToken)
    {
        if (adminUser.IsSSOAccount)
            return;

        Log.Information("Admin user {UserId} does not have SSO enabled, enabling it now", adminUser.Id);

        var userToUpdate = await _db.DomainUsers.FindAsync(new object[] { adminUser.Id }, cancellationToken);
        if (userToUpdate == null)
            return;

        userToUpdate.EnableSSO();
        await _db.SaveChangesAsync(cancellationToken);
        Log.Information("SSO enabled for user {UserId}", adminUser.Id);
    }

    private IEnumerable<UrlValue> GetDefaultAllowedRedirectUris()
    {
        var redirectUris = _appSettings.OAuth.DefaultApplicationAllowedRedirectUris
            .Where(uri =>
                !string.IsNullOrWhiteSpace(uri) &&
                !uri.Contains("${", StringComparison.Ordinal))
            .Select(uri => UrlValue.Create(uri.Trim()))
            .ToList();

        if (redirectUris.Count == 0)
        {
            throw new InvalidOperationException(
                "AppSettings:OAuth:DefaultApplicationAllowedRedirectUris must be configured before seeding the application manager.");
        }

        return redirectUris;
    }

    private static string GenerateRandomSecret()
    {
        var bytes = new byte[32];
        using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
        {
            rng.GetBytes(bytes);
        }
        return Convert.ToBase64String(bytes);
    }
}
