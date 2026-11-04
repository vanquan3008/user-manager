using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.Api.Authorization;
using UserService.Api.Contracts.Account;
using UserService.Application.Interfaces.Services;
using UserService.Domain.Interfaces;
using UserService.Domain.Repositories;

namespace UserService.Api.Controllers;

public sealed record DailyLoginPoint(DateOnly Day, int Succeeded, int Failed);

public sealed record DashboardSummaryDto(
    UserStatistics Users,
    int Applications,
    int ActiveSessions,
    double MfaAdoptionRate,
    IReadOnlyList<DailyLoginPoint> LoginActivity,
    IReadOnlyList<AuditLogDto> RecentSecurityEvents,
    DateTime GeneratedAt);

[ApiController]
[Route("api/dashboard")]
[Authorize(Policy = AuthorizationPolicies.ViewUsers)]
[Produces("application/json")]
public sealed class DashboardController : ApiControllerBase
{
    private const int ActivityDays = 14;

    private readonly IIdentityService _identity;
    private readonly IApplicationRepository _applications;
    private readonly IUserSessionRepository _sessions;
    private readonly IAuditLogRepository _auditLogs;
    private readonly IClock _clock;

    public DashboardController(
        IIdentityService identity,
        IApplicationRepository applications,
        IUserSessionRepository sessions,
        IAuditLogRepository auditLogs,
        IClock clock)
    {
        _identity = identity;
        _applications = applications;
        _sessions = sessions;
        _auditLogs = auditLogs;
        _clock = clock;
    }

    [HttpGet("summary")]
    public async Task<ActionResult<ApiResponse<DashboardSummaryDto>>> Summary(CancellationToken cancellationToken)
    {
        var now = _clock.UtcNow;
        var from = now.Date.AddDays(-(ActivityDays - 1));

        var users = await _identity.GetUserStatisticsAsync(now.AddDays(-30), cancellationToken);
        var apps = await _applications.GetAllActiveWithOwnerAsync(cancellationToken);
        var activeSessions = await _sessions.CountActiveAsync(now, cancellationToken);
        var stats = await _auditLogs.GetDailyLoginStatsAsync(from, cancellationToken);
        var recent = await _auditLogs.GetRecentAsync(8, cancellationToken: cancellationToken);

        // Fill gaps so the chart always has one point per day.
        var byDay = stats.ToDictionary(s => s.Day);
        var series = Enumerable.Range(0, ActivityDays)
            .Select(i => DateOnly.FromDateTime(from.AddDays(i)))
            .Select(d => byDay.TryGetValue(d, out var s) ? new DailyLoginPoint(d, s.Succeeded, s.Failed) : new DailyLoginPoint(d, 0, 0))
            .ToList();

        var adoption = users.Total == 0 ? 0 : Math.Round(users.MfaEnabled * 100.0 / users.Total, 1);
        return OkResponse(new DashboardSummaryDto(
            users, apps.Count, activeSessions, adoption, series,
            recent.Select(AuditLogMapping.ToDto).ToList(), now));
    }
}
