using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using UserService.Application.Configuration;
using UserService.Application.Interfaces.Services.Security;
using UserService.Domain.Interfaces;
using UserService.Domain.Repositories;

namespace UserService.Infrastructure.Services;

/// <summary>
/// Periodic security housekeeping:
/// reloads the signing key ring (and rotates the active key when due), purges expired sessions,
/// authorization codes and refresh tokens, and enforces audit-log retention.
/// </summary>
public sealed class SecurityMaintenanceService : BackgroundService
{
    private static readonly TimeSpan KeyRefreshInterval = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan CleanupInterval = TimeSpan.FromHours(1);
    private static readonly TimeSpan ExpiredGracePeriod = TimeSpan.FromDays(7);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ISigningKeyProvider _keys;
    private readonly IClock _clock;
    private readonly AppSettings _settings;
    private readonly ILogger<SecurityMaintenanceService> _logger;

    public SecurityMaintenanceService(
        IServiceScopeFactory scopeFactory,
        ISigningKeyProvider keys,
        IClock clock,
        AppSettings settings,
        ILogger<SecurityMaintenanceService> logger)
    {
        _scopeFactory = scopeFactory;
        _keys = keys;
        _clock = clock;
        _settings = settings;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var nextCleanup = DateTime.MinValue;
        using var timer = new PeriodicTimer(KeyRefreshInterval);
        do
        {
            try
            {
                await _keys.RefreshAsync(stoppingToken);

                if (_clock.UtcNow >= nextCleanup)
                {
                    await CleanupAsync(stoppingToken);
                    nextCleanup = _clock.UtcNow.Add(CleanupInterval);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Security maintenance cycle failed");
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task CleanupAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var services = scope.ServiceProvider;
        var cutoff = _clock.UtcNow - ExpiredGracePeriod;

        var sessions = await services.GetRequiredService<IUserSessionRepository>().DeleteExpiredAsync(cutoff, cancellationToken);
        var codes = await services.GetRequiredService<IOAuthAuthorizationCodeRepository>().DeleteExpiredAsync(cutoff, cancellationToken);
        var tokens = await services.GetRequiredService<IOAuthRefreshTokenRepository>().DeleteExpiredAsync(cutoff, cancellationToken);
        var audit = _settings.Security.AuditLogRetentionDays > 0
            ? await services.GetRequiredService<IAuditLogRepository>().DeleteOlderThanAsync(
                _clock.UtcNow.AddDays(-_settings.Security.AuditLogRetentionDays), cancellationToken)
            : 0;

        if (sessions + codes + tokens + audit > 0)
            _logger.LogInformation(
                "Security cleanup removed {Sessions} sessions, {Codes} authorization codes, {Tokens} refresh tokens, {Audit} audit records",
                sessions, codes, tokens, audit);
    }
}
