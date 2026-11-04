using OpenIddict.Abstractions;
using Serilog;
using Microsoft.AspNetCore.Identity;
using UserService.Application.Services;
using UserService.Domain.Repositories;
using UserService.Infrastructure.Data;
using UserService.Infrastructure.Identity;
using UserService.Infrastructure.Seeding;
using UserService.Application.Configuration;
using UserService.Application.Interfaces.Services;
using UserService.Domain.Constants;
using UserService.Domain.Interfaces;

namespace UserService.Api.Extensions;

public static class SeedingExtensions
{
    public static async Task<WebApplication> SeedDatabaseAsync(this WebApplication app, CancellationToken cancellationToken = default)
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;
        var dbContext = services.GetRequiredService<UserDbContext>();

        try
        {
            Log.Information("Starting database seeding");
            var roleManager = services.GetRequiredService<RoleManager<AppRole>>();
            var systemRoleSeeder = new SystemRoleSeeder(roleManager);
            await systemRoleSeeder.SeedAsync(cancellationToken);
            var permissionSeeder = new PermissionSeeder(dbContext);
            await permissionSeeder.SeedPermissionsAsync(cancellationToken);
            var featureSeeder = new FeatureSeeder(dbContext);
            await featureSeeder.SeedAsync(cancellationToken);
            var adminRolePermissionSeeder = new AdminRolePermissionSeeder(dbContext);
            await adminRolePermissionSeeder.SeedAsync(cancellationToken);

            var adminSeeder = new AdminSeeder(
                dbContext,
                services.GetRequiredService<IIdentityService>(),
                services.GetRequiredService<IUserRepository>(),
                services.GetRequiredService<IUnitOfWork>(),
                services.GetRequiredService<IClock>()
            );
            await adminSeeder.SeedAdminUserAsync(services);

            var userSeeder = new Infrastructure.Seeding.UserSeeder(
                dbContext,
                services.GetRequiredService<IIdentityService>(),
                services.GetRequiredService<IUserRepository>(),
                services.GetRequiredService<IUnitOfWork>(),
                services.GetRequiredService<IConfiguration>(),
                services.GetRequiredService<IClock>()
            );
            await userSeeder.SeedUsersAsync(services);
            var oauthScopeSeeder = new OAuthScopeSeeder(dbContext);
            await oauthScopeSeeder.SeedAsync(cancellationToken);

            var applicationManagerSeeder = new ApplicationManagerSeeder(
                dbContext,
                services.GetRequiredService<AppSettings>(),
                services.GetRequiredService<IConfiguration>(),
                services.GetRequiredService<IHashingService>(),
                services.GetRequiredService<IClock>());
            await applicationManagerSeeder.SeedAsync(cancellationToken);
            await SeedOpenIddictClientsAsync(services, cancellationToken);

            Log.Information("Database seeding completed successfully");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error during database seeding");
            throw;
        }

        return app;
    }

    private static async Task SeedOpenIddictClientsAsync(IServiceProvider services, CancellationToken cancellationToken)
    {
        var manager = services.GetRequiredService<IOpenIddictApplicationManager>();
        var configuration = services.GetRequiredService<IConfiguration>();

        // Read from configuration
        var clientId = configuration["OpenIddict:ClientId"]
            ?? Environment.GetEnvironmentVariable("OPENIDDICT_CLIENT_ID")
            ?? "user-service-client";

        var clientSecret = configuration["OpenIddict:ClientSecret"]
            ?? Environment.GetEnvironmentVariable("OPENIDDICT_CLIENT_SECRET")
            ?? Environment.GetEnvironmentVariable("OIDC_CLIENT_SECRET")
            ?? string.Empty;

        var redirectUrisStr = configuration["OpenIddict:RedirectUris"]
            ?? Environment.GetEnvironmentVariable("OPENIDDICT_REDIRECT_URIS")
            ?? string.Empty;

        var postLogoutRedirectUrisStr = configuration["OpenIddict:PostLogoutRedirectUris"]
            ?? Environment.GetEnvironmentVariable("OPENIDDICT_POST_LOGOUT_REDIRECT_URIS")
            ?? string.Empty;

        if (await manager.FindByClientIdAsync(clientId, cancellationToken) != null)
        {
            Log.Information("OpenIddict client '{ClientId}' already exists, skipping seeding.", clientId);
            return;
        }

        if (string.IsNullOrWhiteSpace(clientSecret) ||
            clientSecret.Contains("change-in-production", StringComparison.OrdinalIgnoreCase) ||
            clientSecret.Contains("change-this", StringComparison.OrdinalIgnoreCase))
        {
            var env = services.GetRequiredService<IHostEnvironment>();
            if (!env.IsDevelopment())
            {
                Log.Warning("OpenIddict client seeding skipped: OpenIddict:ClientSecret is missing or a placeholder.");
                return;
            }

            Log.Warning("OpenIddict client seeding: ClientSecret not configured. Generating a random development secret.");
            clientSecret = Convert.ToHexString(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32));
        }

        var descriptor = new OpenIddictApplicationDescriptor
        {
            ClientId = clientId,
            ClientSecret = clientSecret,
            DisplayName = "User Manager Web Application",
            ClientType = OpenIddictConstants.ClientTypes.Confidential,
            Permissions =
            {
                OpenIddictConstants.Permissions.Endpoints.Token,
                OpenIddictConstants.Permissions.Endpoints.Authorization,
                OpenIddictConstants.Permissions.Endpoints.Logout,
                OpenIddictConstants.Permissions.Endpoints.Introspection,
                OpenIddictConstants.Permissions.Endpoints.Revocation,
                
                OpenIddictConstants.Permissions.GrantTypes.Password,
                OpenIddictConstants.Permissions.GrantTypes.RefreshToken,
                OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode,
                OpenIddictConstants.Permissions.GrantTypes.ClientCredentials,
                OpenIddictConstants.Permissions.Prefixes.GrantType + OAuthGrantTypes.MagicLink,
                
                OpenIddictConstants.Permissions.ResponseTypes.Code,
                
                OpenIddictConstants.Permissions.Prefixes.Scope + "openid",
                OpenIddictConstants.Permissions.Prefixes.Scope + "profile",
                OpenIddictConstants.Permissions.Prefixes.Scope + "email",
                OpenIddictConstants.Permissions.Prefixes.Scope + "roles"
            }
        };

        if (!string.IsNullOrWhiteSpace(redirectUrisStr))
        {
            foreach (var uri in redirectUrisStr.Split(',', StringSplitOptions.RemoveEmptyEntries))
            {
                descriptor.RedirectUris.Add(new Uri(uri.Trim()));
            }
        }

        if (!string.IsNullOrWhiteSpace(postLogoutRedirectUrisStr))
        {
            foreach (var uri in postLogoutRedirectUrisStr.Split(',', StringSplitOptions.RemoveEmptyEntries))
            {
                descriptor.PostLogoutRedirectUris.Add(new Uri(uri.Trim()));
            }
        }

        await manager.CreateAsync(descriptor, cancellationToken);

        Log.Information("OpenIddict client '{ClientId}' seeded with SSO permissions.", clientId);
    }
}
