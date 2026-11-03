using UserService.Domain.Entities;

namespace UserService.Domain.Repositories;

public sealed record AuditLogQuery(
    int Page = 1,
    int PageSize = 25,
    string? Search = null,
    string? Category = null,
    string? EventType = null,
    bool? Succeeded = null,
    Guid? UserId = null,
    DateTime? From = null,
    DateTime? To = null,
    IReadOnlyCollection<string>? Categories = null);

public sealed record DailyAuthStat(DateOnly Day, int Succeeded, int Failed);

public interface IAuditLogRepository
{
    /// <summary>Persists the entry immediately, independent of the caller's unit of work.</summary>
    Task WriteAsync(AuditLog entry, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<AuditLog> Items, int TotalCount)> QueryAsync(AuditLogQuery query, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DailyAuthStat>> GetDailyLoginStatsAsync(DateTime fromUtc, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AuditLog>> GetRecentAsync(int take, string? category = null, CancellationToken cancellationToken = default);
    Task<int> DeleteOlderThanAsync(DateTime cutoffUtc, CancellationToken cancellationToken = default);
}
