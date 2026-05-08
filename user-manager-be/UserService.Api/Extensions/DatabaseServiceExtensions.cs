using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using UserService.Application.Configuration;
using UserService.Infrastructure.Data;

namespace UserService.Api.Extensions;

public static class DatabaseServiceExtensions
{
    public static IServiceCollection AddDatabase(
        this IServiceCollection services,
        IConfiguration configuration,
        AppSettings appSettings)
    {
        services.AddDbContext<UserDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("UserDatabase")
                ?? throw new InvalidOperationException(
                    "Connection string 'UserDatabase' is required. Set ConnectionStrings:UserDatabase or env ConnectionStrings__UserDatabase.");

            options.UseNpgsql(connectionString);
            options.ConfigureWarnings(w =>
            {
                // Soft-delete query filters on required relationships can trigger this warning.
                // We intentionally use global filters and add matching filters where needed.
                w.Ignore(CoreEventId.PossibleIncorrectRequiredNavigationWithQueryFilterInteractionWarning);
            });
            options.EnableSensitiveDataLogging(appSettings.Database.EnableSensitiveDataLogging);
            options.EnableDetailedErrors(appSettings.Database.EnableDetailedErrors);
            options.UseOpenIddict<Guid>();

        });

        return services;
    }

    public static async Task MigrateDatabaseAsync(this WebApplication app)
    {
        try
        {
            using var scope = app.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<UserDbContext>();
            await dbContext.Database.MigrateAsync();
            app.Logger.LogInformation("UserService: Database migrations applied.");
        }
        catch (Exception ex)
        {
            app.Logger.LogError(
                ex,
                "UserService: Migration failed. Ensure PostgreSQL is running and credentials match. " +
                "Override with env ConnectionStrings__UserDatabase. " +
                "Local Docker: from repo root run `docker compose up -d postgres` then use Password=postgres in the connection string.");
            throw;
        }
    }
}
