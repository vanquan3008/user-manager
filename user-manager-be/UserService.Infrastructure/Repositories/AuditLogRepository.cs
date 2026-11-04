using Microsoft.EntityFrameworkCore;
using UserService.Domain.Entities;
using UserService.Domain.Interfaces;
using UserService.Domain.Repositories;
using UserService.Infrastructure.Data;

namespace UserService.Infrastructure.Repositories;

public sealed class AuditLogRepository : IAuditLogRepository
{
    private readonly UserDbContext _db;
    private readonly DbContextOptions<UserDbContext> _options;
    private readonly IClock _clock;

    public AuditLogRepository(UserDbContext db, DbContextOptions<UserDbContext> options, IClock clock)
    {
        _db = db;
        _options = options;
        _clock = clock;
    }

    public async Task WriteAsync(AuditLog entry, CancellationToken cancellationToken = default)
    {
        // A dedicated context guarantees audit records are persisted even when the caller's
        // unit of work fails, and that unrelated pending changes are never committed by accident.
        await using var context = new UserDbContext(_options, _clock);
        context.AuditLogs.Add(entry);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<(IReadOnlyList<AuditLog> Items, int TotalCount)> QueryAsync(AuditLogQuery query, CancellationToken cancellationToken = default)
    {
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 200);
        var q = _db.AuditLogs.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Category))
            q = q.Where(x => x.Category == query.Category);
        if (query.Categories is { Count: > 0 })
        {
            var categories = query.Categories.ToArray();
            q = q.Where(x => categories.Contains(x.Category));
        }
        if (!string.IsNullOrWhiteSpace(query.EventType))
            q = q.Where(x => x.EventType == query.EventType);
        if (query.Succeeded.HasValue)
            q = q.Where(x => x.Succeeded == query.Succeeded.Value);
        if (query.UserId.HasValue)
            q = q.Where(x => x.ActorUserId == query.UserId || x.TargetUserId == query.UserId);
        if (query.From.HasValue)
            q = q.Where(x => x.OccurredAt >= query.From.Value);
        if (query.To.HasValue)
            q = q.Where(x => x.OccurredAt <= query.To.Value);
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim().ToLower();
            q = q.Where(x =>
                (x.ActorEmail != null && x.ActorEmail.Contains(term)) ||
                (x.IpAddress != null && x.IpAddress.Contains(term)) ||
                x.EventType.Contains(term) ||
                (x.TargetId != null && x.TargetId.ToLower().Contains(term)));
        }

        var total = await q.CountAsync(cancellationToken);
        var items = await q
            .OrderByDescending(x => x.OccurredAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
        return (items, total);
    }

    public async Task<IReadOnlyList<DailyAuthStat>> GetDailyLoginStatsAsync(DateTime fromUtc, CancellationToken cancellationToken = default)
    {
        var loginEvents = new[]
        {
            AuditEventTypes.LoginSucceeded,
            AuditEventTypes.LoginFailed,
            AuditEventTypes.MfaFailed,
            AuditEventTypes.MagicLinkLogin
        };

        var rows = await _db.AuditLogs.AsNoTracking()
            .Where(x => x.OccurredAt >= fromUtc && loginEvents.Contains(x.EventType))
            .GroupBy(x => new { x.OccurredAt.Date, x.Succeeded })
            .Select(g => new { g.Key.Date, g.Key.Succeeded, Count = g.Count() })
            .ToListAsync(cancellationToken);

        return rows
            .GroupBy(r => DateOnly.FromDateTime(r.Date))
            .Select(g => new DailyAuthStat(
                g.Key,
                g.Where(x => x.Succeeded).Sum(x => x.Count),
                g.Where(x => !x.Succeeded).Sum(x => x.Count)))
            .OrderBy(x => x.Day)
            .ToList();
    }

    public async Task<IReadOnlyList<AuditLog>> GetRecentAsync(int take, string? category = null, CancellationToken cancellationToken = default)
    {
        var q = _db.AuditLogs.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(category))
            q = q.Where(x => x.Category == category);
        return await q.OrderByDescending(x => x.OccurredAt).Take(Math.Clamp(take, 1, 100)).ToListAsync(cancellationToken);
    }

    public Task<int> DeleteOlderThanAsync(DateTime cutoffUtc, CancellationToken cancellationToken = default)
        => _db.AuditLogs.Where(x => x.OccurredAt < cutoffUtc).ExecuteDeleteAsync(cancellationToken);
}
