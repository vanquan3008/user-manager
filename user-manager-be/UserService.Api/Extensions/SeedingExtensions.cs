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
            await SubscriptionRoleTypeSeeder.SeedAsync(dbContext);
            var roleManager = services.GetRequiredService<RoleManager<AppRole>>();
            var systemRoleSeeder = new SystemRoleSeeder(roleManager);
            await systemRoleSeeder.SeedAsync(cancellationToken);
            var permissionSeeder = new PermissionSeeder(dbContext);
            await permissionSeeder.SeedPermissionsAsync(cancellationToken);
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
            //var oauthScopeSeeder = new OAuthScopeSeeder(dbContext);
            //await oauthScopeSeeder.SeedAsync(cancellationToken);

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

        if (await manager.FindByClientIdAsync("user-service-client", cancellationToken) != null)
            return;

        // Read from configuration (supports both appsettings.json and environment variables)
        var clientSecret = configuration["OpenIddict:ClientSecret"]
            ?? Environment.GetEnvironmentVariable("OPENIDDICT_CLIENT_SECRET")
            ?? Environment.GetEnvironmentVariable("OIDC_CLIENT_SECRET")
            ?? string.Empty;

        if (string.IsNullOrWhiteSpace(clientSecret) ||
            clientSecret.Contains("change-in-production", StringComparison.OrdinalIgnoreCase) ||
            clientSecret.Contains("change-this", StringComparison.OrdinalIgnoreCase))
        {
            Log.Warning("OpenIddict client seeding skipped: OpenIddict:ClientSecret not properly configured. Using default for development only.");
            clientSecret = "dev-only-secret-not-for-production-use";
        }

        await manager.CreateAsync(new OpenIddictApplicationDescriptor
        {
            ClientId = "user-service-client",
            ClientSecret = clientSecret,
            DisplayName = "User Service (OIDC client)",
            Permissions =
            {
                OpenIddictConstants.Permissions.Endpoints.Token,
                OpenIddictConstants.Permissions.GrantTypes.Password,
                OpenIddictConstants.Permissions.GrantTypes.RefreshToken,
                OpenIddictConstants.Permissions.Prefixes.GrantType + OAuthGrantTypes.MagicLink,
                OpenIddictConstants.Permissions.Prefixes.Scope + "openid",
                OpenIddictConstants.Permissions.Prefixes.Scope + "profile"
            }
        }, cancellationToken);

        Log.Information("OpenIddict client 'user-service-client' seeded.");
    }
}
