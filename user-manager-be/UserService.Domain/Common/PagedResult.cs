namespace UserService.Domain.Common;

public record PagedResult<T>(IEnumerable<T> Items, int TotalCount);
