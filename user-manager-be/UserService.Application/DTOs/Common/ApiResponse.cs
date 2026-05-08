namespace UserService.Application.DTOs.Common;

public record ErrorDetail(string Code, string Message, IDictionary<string, string[]>? Errors = null);

public record ApiResponse<T>(
    bool Success,
    T? Data = default,
    string? Message = null,
    ErrorDetail? Error = null
);

public record PagedResponse<T>(
    bool Success,
    IEnumerable<T> Data,
    int TotalCount,
    int PageNumber,
    int PageSize,
    string? Message = null,
    ErrorDetail? Error = null
) : ApiResponse<IEnumerable<T>>(Success, Data, Message, Error);
