using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.Api.Authorization;
using UserService.Api.Contracts.Account;
using UserService.Domain.Repositories;

namespace UserService.Api.Controllers;

[ApiController]
[Route("api/audit-logs")]
[Authorize(Policy = AuthorizationPolicies.ViewAuditLogs)]
[Produces("application/json")]
public sealed class AuditLogsController : ApiControllerBase
{
    private readonly IAuditLogRepository _auditLogs;

    public AuditLogsController(IAuditLogRepository auditLogs) => _auditLogs = auditLogs;

    [HttpGet]
    public async Task<ActionResult<PagedResponse<AuditLogDto>>> Get(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        [FromQuery] string? search = null,
        [FromQuery] string? category = null,
        [FromQuery] string? eventType = null,
        [FromQuery] bool? succeeded = null,
        [FromQuery] Guid? userId = null,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 200);
        var (items, total) = await _auditLogs.QueryAsync(
            new AuditLogQuery(page, pageSize, search, category, eventType, succeeded, userId, ToUtc(from), ToUtc(to)), cancellationToken);
        return Ok(new PagedResponse<AuditLogDto>(true, items.Select(AuditLogMapping.ToDto).ToList(), total, page, pageSize));
    }

    /// <summary>CSV export of the filtered audit trail (max 5,000 rows).</summary>
    [HttpGet("export")]
    [Produces("text/csv")]
    public async Task<IActionResult> Export(
        [FromQuery] string? search = null,
        [FromQuery] string? category = null,
        [FromQuery] bool? succeeded = null,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        CancellationToken cancellationToken = default)
    {
        var sb = new StringBuilder("occurred_at,event_type,succeeded,actor_email,actor_user_id,target_type,target_id,ip_address,reason,correlation_id\n");
        for (var page = 1; page <= 25; page++)
        {
            var (items, _) = await _auditLogs.QueryAsync(
                new AuditLogQuery(page, 200, search, category, null, succeeded, null, ToUtc(from), ToUtc(to)), cancellationToken);
            foreach (var x in items)
            {
                sb.Append(string.Join(',',
                    x.OccurredAt.ToString("O", CultureInfo.InvariantCulture), Csv(x.EventType), x.Succeeded ? "true" : "false",
                    Csv(x.ActorEmail), x.ActorUserId, Csv(x.TargetType), Csv(x.TargetId), Csv(x.IpAddress), Csv(x.Reason), Csv(x.CorrelationId)));
                sb.Append('\n');
            }
            if (items.Count < 200)
                break;
        }

        return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", $"audit-log-{DateTime.UtcNow:yyyyMMddHHmmss}.csv");
    }

    private static DateTime? ToUtc(DateTime? value)
        => value is null ? null : value.Value.Kind == DateTimeKind.Utc ? value : DateTime.SpecifyKind(value.Value.ToUniversalTime(), DateTimeKind.Utc);

    /// <summary>Escapes CSV and neutralises spreadsheet formula injection.</summary>
    private static string Csv(string? value)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;
        if ("=+-@\t\r".Contains(value[0])) value = "'" + value;
        return value.IndexOfAny(new[] { ',', '"', '\n', '\r' }) >= 0 ? $"\"{value.Replace("\"", "\"\"")}\"" : value;
    }
}
