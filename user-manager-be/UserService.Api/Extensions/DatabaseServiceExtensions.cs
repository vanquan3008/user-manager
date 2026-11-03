using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using UserService.Application.Configuration;
using UserService.Infrastructure.Data;
using Npgsql;

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
            var connectionString = ResolveConnectionString(configuration)
                ?? throw new InvalidOperationException(
                    "Connection string 'UserDatabase' is required. Set ConnectionStrings:UserDatabase, DATABASE_URL, or POSTGRES_* environment variables.");

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

    private static string? ResolveConnectionString(IConfiguration configuration)
    {
        var explicitConnectionString = Environment.GetEnvironmentVariable("ConnectionStrings__UserDatabase");
        if (!string.IsNullOrWhiteSpace(explicitConnectionString))
            return explicitConnectionString;

        var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
        if (!string.IsNullOrWhiteSpace(databaseUrl))
            return databaseUrl;

        var postgresUser = Environment.GetEnvironmentVariable("POSTGRES_USER");
        var postgresPassword = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD");
        var postgresDatabase = Environment.GetEnvironmentVariable("POSTGRES_DB");
        if (!string.IsNullOrWhiteSpace(postgresUser) &&
            !string.IsNullOrWhiteSpace(postgresPassword) &&
            !string.IsNullOrWhiteSpace(postgresDatabase))
        {
            var builder = new NpgsqlConnectionStringBuilder
            {
                Host = Environment.GetEnvironmentVariable("POSTGRES_HOST") ?? "localhost",
                Port = int.TryParse(Environment.GetEnvironmentVariable("POSTGRES_PORT"), out var port) ? port : 5432,
                Database = postgresDatabase,
                Username = postgresUser,
                Password = postgresPassword,
                SslMode = SslMode.Disable
            };

            return builder.ConnectionString;
        }

        return configuration.GetConnectionString("UserDatabase");
    }

    public static async Task MigrateDatabaseAsync(this WebApplication app)
    {
        try
        {
            using var scope = app.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<UserDbContext>();
            if (dbContext.Database.IsRelational())
                await dbContext.Database.MigrateAsync();
            else
                await dbContext.Database.EnsureCreatedAsync(); // non-relational providers (integration tests)
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
