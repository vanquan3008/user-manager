using System.Net;

namespace UserService.Application.DTOs.Common;

public record ThirdPartyApiResponse(
    bool Success,
    HttpStatusCode StatusCode,
    string? RawResponse = null,
    string? ErrorMessage = null);

public sealed record ThirdPartyApiResponse<T>(
    bool Success,
    HttpStatusCode StatusCode,
    T? Data = default,
    string? RawResponse = null,
    string? ErrorMessage = null) : ThirdPartyApiResponse(Success, StatusCode, RawResponse, ErrorMessage);