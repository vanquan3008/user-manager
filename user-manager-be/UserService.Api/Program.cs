using Serilog;
using UserService.Api.Extensions;
using UserService.Application.Configuration;
using UserService.Application.DependencyInjection;
using Microsoft.Extensions.Hosting;
using UserService.Infrastructure.DependencyInjection;

Log.Logger = SerilogExtensions.CreateBootstrapLogger();

try
{
    Log.Information("Starting UserService.Api");
    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseCustomSerilog();
    var appSettings = new AppSettings();
    builder.Configuration.GetSection(AppSettings.SectionName).Bind(appSettings);

    builder.Services.AddApplicationServices(builder.Configuration);
    builder.Services.AddInfrastructureServices(appSettings);                  
    builder.Services.AddApiServices(builder.Configuration, appSettings);
   
    var app = builder.Build();
   
    if (appSettings.IsAutoMigrate)
    {
        if (appSettings.IsProduction())
        {
            Log.Warning("Auto-migration is enabled in production environment. This is not recommended.");
            Log.Warning("Consider running migrations through CI/CD pipeline instead.");
        }
        
        Log.Information("Starting database migration and seeding");
        await app.MigrateDatabaseAsync();
        await app.SeedDatabaseAsync();
        Log.Information("Database migration and seeding completed");
    }
    else
    {
        Log.Information("Auto-migration is disabled. Skipping database migration and seeding.");
    }
    
    app.UseApiPipeline(appSettings);

    app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
